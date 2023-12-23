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

namespace jmayberry.TypewriterHelper {

	public enum DialogOption {
		Unknown,
		NextEntry,
		SkipPopulate,
		Close,
		SelectOption,
	}

	[Serializable]
	public class  ChatBubbleInfo {
        [Required] public readonly Sprite background;
        [Required] public readonly Sprite fallbackIconSprite;
        [SerializedDictionary("Dialog Option", "Icon")] public readonly SerializedDictionary<DialogOption, Sprite> iconSprite;
	}

	public abstract class DialogManagerBase<SpeakerType> : EventManagerBase where SpeakerType : Enum {
		[Header("Setup")]
		[SerializedDictionary("Speaker Type", "Chat Bubble")] public SerializedDictionary<SpeakerType, ChatBubbleInfo> chatBubbleInfo = new SerializedDictionary<SpeakerType, ChatBubbleInfo>();
		public ChatBubbleInfo fallbackChatBubbleInfo;
        [Required] [SerializeField] private ChatBubbleBase<SpeakerType> chatBubblePrefab;
		[EntryFilter(Variant = EntryVariant.Fact)] public EntryReference fallbackSpeakerReference;

        [Header("For Debugging")]
		public static readonly DialogContext defaultContext = new DialogContext();
        [SerializeField] internal static Dictionary<int, Speaker<SpeakerType>> speakerLookup;

        protected TypewriterWatcher typewriterWatcher;
		protected internal static UnitySpawner<ChatBubbleBase<SpeakerType>> chatBubbleSpawner;
        protected internal static CodeSpawner<DialogSequenceBase<SpeakerType>> dialogSequenceSpawner;

        public static DialogManagerBase<SpeakerType> instance { get; private set; }

		private void Awake() {
			if (instance != null && instance != this) {
				Debug.LogError("Found more than one DialogManager<SpeakerType> in the scene.");
				Destroy(this.gameObject);
				return;
			}

			instance = this;

			speakerLookup = new Dictionary<int, Speaker<SpeakerType>>();
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

            // TODO: Check priority here

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
				Debug.Log($"@Manager.HandleTypewriterEvent.fact; {factEntry}");
                dialogContext.TryInvoke(factEntry);
                return;
			}

			// Rules are part of the current sequence
			if (entry is RuleEntry ruleEntry) {
				Debug.Log($"@Manager.HandleTypewriterEvent.rule; {ruleEntry}");
                return;
            }

			// Events spawn a new sequence
			if (entry is EventEntry eventEntry) {
				Debug.Log($"@Manager.HandleTypewriterEvent.event; {eventEntry}");
				this.TryStartSequence(dialogContext, eventEntry);
            }

            Debug.LogError($"Unknown entry type {entry}");
		}
	}
}