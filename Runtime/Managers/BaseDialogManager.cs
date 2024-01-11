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
	public class BaseChatBubbleInfo<EmotionType> where EmotionType : Enum {
		[Required] public Sprite fallbackBackground;
		[Required] public Sprite fallbackIconSprite;
		[Required] public BaseSpeakerVoice fallbackSpeakerVoice;
		[SerializedDictionary("Dialog Option", "Icon")] public SerializedDictionary<DialogOption, Sprite> iconSprite;
		[SerializedDictionary("Chat Type", "Background")] public SerializedDictionary<ChatBubbleType, Sprite> backgroundSprite;
		[SerializedDictionary("Emotion", "Speaker Voice")] public SerializedDictionary<EmotionType, BaseSpeakerVoice> speakerVoice;
	}

	public abstract class BaseDialogManager<SpeakerType, EmotionType> : EventManagerBase where SpeakerType : Enum where EmotionType : Enum {
		[Header("Base: Setup")]
		[EntryFilter(Variant = EntryVariant.Fact)] public EntryReference fallbackSpeakerReference;

		[Header("Base: Tweak")]
		[SerializeField] protected float InteractionCooldown = 0.1f;
		[Readonly][SerializeField] public float lastInteractionTime = 0f;
		[SerializeField] protected float UpdatePositionCooldown = 0.1f;
		[Readonly][SerializeField] public float lastUpdatePositionTime = 0f;

		[Header("Base: For Debugging")]
		public static readonly DialogContext defaultContext = new DialogContext();
		[SerializeField] internal static Dictionary<int, Speaker<SpeakerType, EmotionType>> speakerLookup = new Dictionary<int, Speaker<SpeakerType, EmotionType>>();
		[Readonly] public List<BaseEntry> orphanedEntries = new List<BaseEntry>();

		protected TypewriterWatcher typewriterWatcher;
		protected internal static UnitySpawner<BaseChat<SpeakerType, EmotionType>> chatBubbleSpawner;

		[Header("Base: Events")]
		[Readonly] public UnityEvent EventUserInteractedWithDialog = new UnityEvent();
		[Readonly] public UnityEvent EventUpdateBubblePosition = new UnityEvent();

		public static BaseDialogManager<SpeakerType, EmotionType> instance { get; private set; }

		protected virtual void Awake() {
			if (instance != null && instance != this) {
				Debug.LogError("Found more than one BaseDialogManager<SpeakerType, EmotionType> in the scene.");
				Destroy(this.gameObject);
				return;
			}

			instance = this;
		}

		protected virtual void OnEnable() {
			TypewriterDatabase.Instance.AddListener(this.HandleTypewriterEvent);
		}

		protected virtual void OnDisable() {
			TypewriterDatabase.Instance.RemoveListener(this.HandleTypewriterEvent);
		}

		protected abstract BaseChatSequence<SpeakerType, EmotionType> SpawnDialogSequence();

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

		public Speaker<SpeakerType, EmotionType> LookupSpeaker(BaseDialogEntry<EmotionType> BaseDialogEntry) {
			var speaker = speakerLookup.GetValueOrDefault(BaseDialogEntry.Speaker.ID);
			if (speaker == null) {
				speaker = speakerLookup.GetValueOrDefault(this.fallbackSpeakerReference.ID);
			}
			return speaker;
		}

		public Speaker<SpeakerType, EmotionType> LookupSpeaker(SpeakerEntry speakerEntry) {
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
			// TODO: Add a cooldown?

			if (!dialogContext.WouldInvoke(eventEntry)) {
				return false;
			}

			if (this.isSequenceRunning && (this.currentSequence is BaseChatSequence<SpeakerType, EmotionType> currentDialogSequence)) {
				if (!currentDialogSequence.ShouldOverride(eventEntry)) {
					return false;
				}
			}

			BaseChatSequence<SpeakerType, EmotionType> dialogSequence = this.SpawnDialogSequence();
			dialogSequence.rootEntry = eventEntry;
			dialogSequence.Reset();

			this.StartSequence(dialogContext, dialogSequence);
			return true;
		}

		protected virtual void HandleTypewriterEvent(BaseEntry entry, ITypewriterContext iContext) {
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