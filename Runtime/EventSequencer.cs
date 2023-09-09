using System.Collections.Generic;
using UnityEngine;
using Aarthificial.Typewriter.References;
using Aarthificial.Typewriter.Tools;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter;
using jmayberry.TypewriterHelper.Entries;

/**
 * This is in charge of catching events and deciding if they should interrupt the current sequence.
 * 
 * Events start a new sequence; rules are clips that belong to the current sequence.
 */
namespace jmayberry.TypewriterHelper.Applications {
        public class EventSequencer : MonoBehaviour {
        [Required][SerializeField] internal Sprite[] chatBubbleSprite;
        [Required][SerializeField] internal Sprite[] chatButtonSprite;
        [Required][SerializeField] internal ChatBubble chatBubblePrefab;

        private TypewriterWatcher typewriterWatcher;
		private DialogueSequence currentSequence;

		[SerializeField] internal Dictionary<int, Speaker> speakerLookup;
		[Readonly][SerializeField] private List<DialogueSequence> activeDialogueSequences;
		[Readonly][SerializeField] private List<DialogueSequence> inactiveDialogueSequences;
        [Readonly][SerializeField] private List<ChatBubble> activeChatBubbles;
        [Readonly][SerializeField] private List<ChatBubble> inactiveChatBubbles;

        public static EventSequencer instance { get; private set; }
		private void Awake() {
			if (instance != null) {
				Debug.LogError("Found more than one Dialogue Sequencer in the scene.");
			}

			instance = this;

			this.speakerLookup = new Dictionary<int, Speaker>();
			this.activeDialogueSequences = new List<DialogueSequence>();
			this.inactiveDialogueSequences = new List<DialogueSequence>();
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

        public Speaker LookupSpeaker(DialogueEntry currentEntry) {
            return this.speakerLookup[currentEntry.Speaker.ID];
        }
        public Speaker LookupSpeaker(DialogueArrayEntryLine currentEntry) {
            return this.speakerLookup[currentEntry.Speaker.ID];
        }

        internal DialogueSequence SpawnDialogueSequence() {
			DialogueSequence dialogueSequence;

			// Try to reuse an inactive dialogueSequence, or create a new one
			if (this.inactiveDialogueSequences.Count > 0) {
				dialogueSequence = this.inactiveDialogueSequences[this.inactiveDialogueSequences.Count - 1];
				this.inactiveDialogueSequences.RemoveAt(this.inactiveDialogueSequences.Count - 1);
				this.activeDialogueSequences.Add(dialogueSequence);
			}
			else {
				dialogueSequence = new DialogueSequence();
			}

			return dialogueSequence;
		}

		internal void DespawnDialogueSequence(DialogueSequence dialogueSequence) {
			this.activeDialogueSequences.Remove(dialogueSequence);
			this.inactiveDialogueSequences.Add(dialogueSequence);
        }

        internal ChatBubble SpawnChatBubble() {
            ChatBubble chatBubble;

            // Try to reuse an inactive chatBubble, or create a new one
            if (this.inactiveChatBubbles.Count > 0) {
                chatBubble = this.inactiveChatBubbles[this.inactiveChatBubbles.Count - 1];
                this.inactiveChatBubbles.RemoveAt(this.inactiveChatBubbles.Count - 1);
                this.activeChatBubbles.Add(chatBubble);
            }
            else {
                chatBubble = Instantiate(this.chatBubblePrefab, this.transform.position, this.transform.rotation);
            }

            return chatBubble;
        }

        internal void DespawnChatBubble(ChatBubble dialogueSequence) {
            this.activeChatBubbles.Remove(dialogueSequence);
            this.inactiveChatBubbles.Add(dialogueSequence);
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