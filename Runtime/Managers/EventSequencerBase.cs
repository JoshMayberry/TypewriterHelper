using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


using Aarthificial.Typewriter.References;
using Aarthificial.Typewriter.Tools;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter;

using jmayberry.TypewriterHelper.Entries;
using jmayberry.CustomAttributes;
using jmayberry.Spawner;

/**
 * This is in charge of catching events and deciding if they should interrupt the current sequence.
 * 
 * Events start a new sequence; rules are clips that belong to the current sequence.
 */
namespace jmayberry.TypewriterHelper {
    public abstract class EventSequencerBaseOld : MonoBehaviour {
        [Required][SerializeField] internal Sprite[] chatBubbleSprite;
        [Required][SerializeField] internal Sprite[] chatButtonSprite;
        [Required][SerializeField] internal ChatBubbleBase chatBubblePrefab;

        private TypewriterWatcher typewriterWatcher;
		private DialogueSequence currentSequence;

        private static UnitySpawner<ChatBubbleBase> chatBubbleSpawner;
        //private static CodeSpawner<DialogueSequence> dialogueSequeneceSpawner;

		[SerializeField] internal static Dictionary<int, ISpeaker> speakerLookup;
		[Readonly][SerializeField] private List<DialogueSequence> activeDialogueSequences;
		[Readonly][SerializeField] private List<DialogueSequence> inactiveDialogueSequences;
        [Readonly][SerializeField] private List<ChatBubbleBase> activeChatBubbles;
        [Readonly][SerializeField] private List<ChatBubbleBase> inactiveChatBubbles;

		private void Awake() {
            if (EventSequencerBase.instance != null && EventSequencerBase.instance != this) {
				Debug.LogError("Found more than one EventSequencerBase in the scene.");
                Destroy(this.gameObject);
                return;
            }

            EventSequencerBase.instance = this;
            EventSequencerBase.speakerLookup = new Dictionary<int, ISpeaker>();
            EventSequencerBase.chatBubbleSpawner = new UnitySpawner<ChatBubbleBase>(this.chatBubblePrefab);
            //EventSequencer.dialogueSequeneceSpawner = new CodeSpawner<DialogueSequence>();
        }

		private void OnEnable() {
			TypewriterDatabase.Instance.AddListener(this.HandleTypewriterEvent);
		}

		private void OnDisable() {
			TypewriterDatabase.Instance.RemoveListener(this.HandleTypewriterEvent);
		}

        public int GetFact(Context context, EntryReference factReference) {
			factReference.TryGetEntry(out FactEntry factEntry);
			return context.Get(factEntry);
		}

		public void SetFact(Context context, EntryReference factReference, int value, bool isSilent = false) {
			factReference.TryGetEntry(out FactEntry factEntry);
			context.Set(factEntry, value);

			if (!isSilent) {
				TypewriterDatabase.Instance.MarkChange();
			}
		}

		public void IncrementFact(Context context, EntryReference factReference, int value, bool isSilent = false) {
			factReference.TryGetEntry(out FactEntry factEntry);
			context.Add(factEntry, value);

			if (!isSilent) {
				TypewriterDatabase.Instance.MarkChange();
			}
		}

        public ISpeaker LookupSpeaker(DialogueEntry currentEntry) {
            return EventSequencer.speakerLookup[currentEntry.Speaker.ID];
        }

        internal DialogueSequence SpawnDialogueSequence() {
            return null;
			//return EventSequencer.dialogueSequeneceSpawner.Spawn();
		}

		internal void DespawnDialogueSequence(DialogueSequence dialogueSequence) {
			//EventSequencer.dialogueSequeneceSpawner.Despawn(dialogueSequence);
        }

        internal ChatBubbleBase SpawnChatBubble() {
			return EventSequencer.chatBubbleSpawner.Spawn();
        }

        internal void DespawnChatBubble(ChatBubbleBase chatBubble) {
			EventSequencer.chatBubbleSpawner.Despawn(chatBubble);
        }

        internal void HandleTypewriterEvent(BaseEntry entry, ITypewriterContext iContext) {
            if (entry is EventEntry eventEntry) {
                if (!iContext.WouldInvoke(entry)) {
                    return;
                }

                if (this.currentSequence != null) {
                    // Check if the priority of this event is bigger than the current event
                    return;

					//this.currentSequence.Cancel();
                }

                this.currentSequence = this.SpawnDialogueSequence();
				this.currentSequence.SetEvent(eventEntry, this.SpawnChatBubble());
                iContext.Process(entry);
				return;
            }

            if (iContext is not Context context) {
                throw new System.Exception("Unknown context type for '" + iContext + "'");
            }

            if (entry is DialogueEntry dialogueEntry) {
                this.currentSequence.Begin(context, dialogueEntry);
                return;
            }

            if (entry is DialogueArrayEntry dialogueArrayEntry) {
                this.currentSequence.Begin(context, dialogueArrayEntry);
                return;
            }

            throw new System.Exception("Unknown entry type for '" + entry + "'");
		}
    }
}