using System;
using System.Collections.Generic;
using UnityEngine;

using AYellowpaper.SerializedCollections;
using Aarthificial.Typewriter.References;
using Aarthificial.Typewriter.Tools;
using Aarthificial.Typewriter.Entries;
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
		public Sprite background;
		[SerializedDictionary("Dialog Option", "Icon")] public SerializedDictionary<DialogOption, Sprite> iconSprite;
	}

	public abstract class DialogManagerBase<SpeakerType> : EventManagerBase where SpeakerType : Enum {
		[Header("Setup")]
		[SerializedDictionary("Speaker Type", "Chat Bubble")] public SerializedDictionary<SpeakerType, ChatBubbleInfo> chatBubbleInfo;
		[Required][SerializeField] internal ChatBubbleBase chatBubblePrefab;

		[Header("For Debugging")]
		[SerializeField] internal static Dictionary<int, ISpeaker> speakerLookup;
		[Readonly] public static DialogContext defaultContext = new DialogContext();

        private TypewriterWatcher typewriterWatcher;
		private static UnitySpawner<ChatBubbleBase> chatBubbleSpawner;
        private static CodeSpawner<DialogSequenceBase> dialogSequenceSpawner;

        public static DialogManagerBase<SpeakerType> instance { get; private set; }

		private void Awake() {
			if (instance != null && instance != this) {
				Debug.LogError("Found more than one DialogManager<SpeakerType> in the scene.");
				Destroy(this.gameObject);
				return;
			}

			instance = this;

			speakerLookup = new Dictionary<int, ISpeaker>();
			chatBubbleSpawner = new UnitySpawner<ChatBubbleBase>();
			dialogSequenceSpawner = new CodeSpawner<DialogSequenceBase>();
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

        public ISpeaker LookupSpeaker(DialogEntry currentEntry) {
			return speakerLookup[currentEntry.Speaker.ID];
        }

        internal ChatBubbleBase SpawnChatBubble() {
            return chatBubbleSpawner.Spawn();
        }

        internal void DespawnChatBubble(ChatBubbleBase chatBubble) {
            chatBubbleSpawner.Despawn(chatBubble);
        }

        internal DialogSequenceBase SpawnDialogSequence(EventEntry eventEntry) {
            DialogSequenceBase dialogSequence = dialogSequenceSpawner.Spawn();
			dialogSequence.SetEventEntry(eventEntry);
			dialogSequence.defaultContext = defaultContext;
            return dialogSequence;
        }

        internal void DespawnDialogSequence(DialogSequenceBase dialogSequence) {
            dialogSequenceSpawner.Despawn(dialogSequence);
        }

        public bool TrySequence(EntryReference eventReference) {
            return this.TrySequence(defaultContext, eventReference);
        }

        public bool TrySequence(DialogContext dialogContext, EntryReference eventReference) {
            eventReference.TryGetEntry(out EventEntry eventEntry);
            return this.TrySequence(dialogContext, eventEntry);
        }

        public bool TrySequence(EventEntry eventEntry) {
			return this.TrySequence(defaultContext, eventEntry);
        }

        public bool TrySequence(DialogContext dialogContext, EventEntry eventEntry) {
            if (!dialogContext.WouldInvoke(eventEntry)) {
                return false;
            }

            DialogSequenceBase dialogSequence = SpawnDialogSequence(eventEntry);
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
			if (entry is RuleEntry triggerEntry) {
                return;
			}

			// Events spawn a new sequence
			if (entry is EventEntry eventEntry) {
				Debug.Log($"@HandleTypewriterEvent.event.0; {eventEntry}");
				this.TrySequence(dialogContext, eventEntry);
            }

            Debug.LogError($"Unknown entry type {entry}");
		}
	}
}