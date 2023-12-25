using System;
using System.Collections.Generic;
using UnityEngine;

using AYellowpaper.SerializedCollections;
using Aarthificial.Typewriter.References;
using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.Tools;
using Aarthificial.Typewriter;

using jmayberry.TypewriterHelper.Entries;
using jmayberry.CustomAttributes;
using jmayberry.EventSequencer;
using jmayberry.Spawner;
using UnityEngine.Events;

namespace jmayberry.TypewriterHelper {

	public enum DialogOption {
		Unknown = 0,
		Interact_Inactive = 101,
		Interact_Active = 102,
		StartDialog_Inactive = 151,
		StartDialog_Active = 152,
		NextEntry = 201,
		SkipPopulate = 202,
		Close = 301,
		SelectOption = 401,
	}

	[Serializable]
	public class  ChatBubbleInfo {
		[Required] public Sprite fallbackBackground;
		[Required] public Sprite fallbackIconSprite;
		[Required] public Sprite fallbackPointToSpeakerSprite;
		[SerializedDictionary("Dialog Option", "Icon")] public SerializedDictionary<DialogOption, Sprite> iconSprite;
		[SerializedDictionary("Chat Type", "Background")] public SerializedDictionary<ChatBubbleType, Sprite> backgroundSprite;
		[SerializedDictionary("Chat Type", "Pointer")] public SerializedDictionary<ChatBubbleType, Sprite> pointToSpeakerSprite;
    }

    public abstract class DialogManagerBase<SpeakerType> : EventManagerBase where SpeakerType : Enum {
		[Header("Setup")]
		[SerializedDictionary("Speaker Type", "Chat Bubble")] public SerializedDictionary<SpeakerType, ChatBubbleInfo> chatBubbleInfo = new SerializedDictionary<SpeakerType, ChatBubbleInfo>();
		public ChatBubbleInfo fallbackChatBubbleInfo;
		[Required] [SerializeField] private ChatBubbleBase<SpeakerType> chatBubblePrefab;
		[EntryFilter(Variant = EntryVariant.Fact)] public EntryReference fallbackSpeakerReference;


		[Header("Tweak")]
		[SerializeField] protected float InteractionCooldown = 0.1f;
		[Readonly][SerializeField] public float lastInteractionTime = 0f;
		[SerializeField] protected float UpdatePositionCooldown = 0.1f;
		[Readonly][SerializeField] public float lastUpdatePositionTime = 0f;

		[Header("For Debugging")]
		public static readonly DialogContext defaultContext = new DialogContext();
		[SerializeField] internal static Dictionary<int, Speaker<SpeakerType>> speakerLookup = new Dictionary<int, Speaker<SpeakerType>>();
		[Readonly] public List<BaseEntry> orphanedEntries = new List<BaseEntry>();

		protected TypewriterWatcher typewriterWatcher;
		protected internal static UnitySpawner<ChatBubbleBase<SpeakerType>> chatBubbleSpawner;
		protected internal static CodeSpawner<DialogSequenceBase<SpeakerType>> dialogSequenceSpawner;
		
		[Header("Events")]
		[Readonly] public UnityEvent EventUserInteractedWithDialog = new UnityEvent();
		[Readonly] public UnityEvent EventUpdateBubblePosition = new UnityEvent();

		public static DialogManagerBase<SpeakerType> instance { get; private set; }

		private void Awake() {
			if (instance != null && instance != this) {
				Debug.LogError("Found more than one DialogManager<SpeakerType> in the scene.");
				Destroy(this.gameObject);
				return;
			}

			instance = this;

			chatBubbleSpawner = new UnitySpawner<ChatBubbleBase<SpeakerType>>(chatBubblePrefab);
			dialogSequenceSpawner = new CodeSpawner<DialogSequenceBase<SpeakerType>>();
		}

		private void OnEnable() {
			TypewriterDatabase.Instance.AddListener(this.HandleTypewriterEvent);
		}

		private void OnDisable() {
			TypewriterDatabase.Instance.RemoveListener(this.HandleTypewriterEvent);
		}

		public int GetFact(ITypewriterContext context, EntryReference factReference) {
			factReference.TryGetEntry(out FactEntry factEntry);
			return context.Get(factEntry);
		}

		public void SetFact(ITypewriterContext context, EntryReference factReference, int value, bool isSilent = false) {
			factReference.TryGetEntry(out FactEntry factEntry);
			context.Set(factEntry, value);

			if (!isSilent) {
				TypewriterDatabase.Instance.MarkChange();
			}
		}

		public void IncrementFact(ITypewriterContext context, EntryReference factReference, int value, bool isSilent = false) {
			factReference.TryGetEntry(out FactEntry factEntry);
			context.Add(factEntry, value);

			if (!isSilent) {
				TypewriterDatabase.Instance.MarkChange();
			}
		}

		public Speaker<SpeakerType> LookupSpeaker(DialogEntry dialogEntry) {
			var speaker = speakerLookup.GetValueOrDefault(dialogEntry.Speaker.ID);
			if (speaker == null) {
				speaker = speakerLookup.GetValueOrDefault(this.fallbackSpeakerReference.ID);
			}
			return speaker;
		}

		public Speaker<SpeakerType> LookupSpeaker(SpeakerEntry speakerEntry) {
			var speaker = speakerLookup.GetValueOrDefault(speakerEntry.ID);
			if (speaker == null) {
				speaker = speakerLookup.GetValueOrDefault(this.fallbackSpeakerReference.ID);
			}
			return speaker;
		}

		public bool TryStartSequence(EntryReference eventReference) {
			return this.TryStartSequence(defaultContext, eventReference);
		}

		public bool TryStartSequence(DialogContext dialogContext, EntryReference eventReference) {
			eventReference.TryGetEntry(out EventEntry eventEntry);
			return this.TryStartSequence(dialogContext, eventEntry);
		}

		public bool TryStartSequence(EventEntry eventEntry) {
			return this.TryStartSequence(defaultContext, eventEntry);
		}

		public bool TryStartSequence(DialogContext dialogContext, EventEntry eventEntry) {
			if (!dialogContext.WouldInvoke(eventEntry)) {
				return false;
			}

			if (this.isSequenceRunning && (this.currentSequence is DialogSequenceBase<SpeakerType> currentDialogSequence)) {
				if (!currentDialogSequence.ShouldOverride(eventEntry)) {
					return false;
				}
			}

			DialogSequenceBase<SpeakerType> dialogSequence = dialogSequenceSpawner.Spawn();
			dialogSequence.rootEntry = eventEntry;
			dialogSequence.Reset();

			this.StartSequence(dialogContext, dialogSequence);
			return true;
		}

		internal void HandleTypewriterEvent(BaseEntry entry, ITypewriterContext iContext) {
			if (iContext is not DialogContext dialogContext) {
				Debug.LogError($"Unknown context type {iContext}");
				return;
			}

			// Facts are things that are true about the world
			if (entry is FactEntry factEntry) {
				dialogContext.TryInvoke(factEntry);
				return;
			}

			// Rules are part of the current sequence
			if (entry is RuleEntry ruleEntry) {
				return;
			}

			// Events spawn a new sequence
			if (entry is EventEntry eventEntry) {
				this.TryStartSequence(dialogContext, eventEntry);
			}

			Debug.LogError($"Unknown entry type {entry}");
		}

		public void OnUserInteractedWithDialog() {
			if (this.lastInteractionTime + this.InteractionCooldown > Time.time) {
				return;
			}
			this.lastInteractionTime = Time.time;
			this.EventUserInteractedWithDialog.Invoke();
		}

		public void OnUpdateBubblePosition() {
			if (this.lastUpdatePositionTime + this.UpdatePositionCooldown > Time.time) {
				return;
			}
			this.lastUpdatePositionTime = Time.time;
			this.EventUpdateBubblePosition.Invoke();
		}
	}
}