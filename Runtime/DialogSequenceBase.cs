using System;
using System.Linq;
using System.Collections;

using UnityEngine;
using UnityEngine.Events;

using Aarthificial.Typewriter;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.References;

using jmayberry.TypewriterHelper.Entries;
using jmayberry.CustomAttributes;
using jmayberry.EventSequencer;
using jmayberry.Spawner;

namespace jmayberry.TypewriterHelper {
	public class DialogSequenceBase<SpeakerType> : SequenceBase where SpeakerType : Enum {
        [Header("Tweak")]
        [SerializeField] protected float InteractionCooldown = 0.1f;
        [Readonly] [SerializeField] public float lastInteractedTime = 0f;

        [Header("Debug")]
        [Readonly] public EventEntry rootEntry;
        [Readonly] public ChatBubbleBase<SpeakerType> chatBubble;
        [Readonly] public BaseEntry currentEntry;
        [Readonly] public bool isStarted;
        [Readonly] public bool isFinished;
        [Readonly] public bool isContinue;

        public override string ToString() {
			return $"<DialogSequenceBase:{this.GetHashCode()}>";
		}

		public DialogSequenceBase() : base() {
		}

		public void Reset() {
			this.isStarted = false;
			this.isContinue = false;
			this.isFinished = false;

			this.currentEntry = null;
        }

        public override void OnSpawn(object spawner) {
			this.CreateNewChatBubble();
            DialogManagerBase<SpeakerType>.instance.UserInteractedWithDialog.AddListener(this.OnUserInteracted);
            base.OnSpawn(spawner);
        }

        public override void OnDespawn(object spawner) {
			this.DoneWithChatBubble();
            DialogManagerBase<SpeakerType>.instance.UserInteractedWithDialog.RemoveListener(this.OnUserInteracted);
            base.OnDespawn(spawner);
        }

        public virtual void OnUserInteracted() {
            if (this.lastInteractedTime + this.InteractionCooldown > Time.time) {
                return;
            }
            this.lastInteractedTime = Time.time;

            if (!this.isStarted) {
                Debug.LogError("Sequence has not started");
                return;
            }

            if (this.isFinished) {
                Debug.LogError("Sequence has already finished");
                return;
            }

            if (this.chatBubble.currentProgress < 1) {
                this.chatBubble.OnSkipToEnd();
            }
            else {
                this.isContinue = true;
            }
        }

        public bool CanStart() {
			if (this.isStarted) {
				Debug.LogError("Sequence has already started");
				return false;
			}

			if (this.isFinished) {
				Debug.LogError("Sequence has already finished");
				return false;
			}

			if (this.rootEntry == null) {
				Debug.LogError("Root Entry not yet set");
				return false;
            }

            if (this.chatBubble == null) {
                Debug.LogError("Chat Bubble not yet set");
                return false;
            }

            return true;
		}

        // See: https://github.com/aarthificial-unity/foundations/blob/7d43e288317085920a55ea61c09bf30f3371b33c/Assets/Player/PlayerController.cs#L121-L138
        public override IEnumerator Start(IContext iContext, Action<SequenceBase> callback) {
			if (!this.CanStart()) {
				yield break;
			}

			if (iContext is not DialogContext dialogContext) {
				Debug.LogError($"Unknown context type {iContext}");
				yield break;
			}

			this.isStarted = true;
			TypewriterDatabase.Instance.AddListener(this.HandleTypewriterEvent);
            dialogContext.Process(this.rootEntry);

			BaseEntry previousEntry = this.rootEntry;
			while (!this.isFinished) {
				yield return new WaitUntil(() => previousEntry != this.currentEntry);

				this.isContinue = false;
				previousEntry = this.currentEntry;
                yield return this.HandleCurrentEntry(dialogContext, this.currentEntry);


                yield return new WaitUntil(() => this.isContinue || this.isFinished);
                this.isContinue = false;

                if (!dialogContext.HasMatchingRule(previousEntry.ID)) {
                    this.isFinished = true;
                }
            }

            DialogManagerBase<SpeakerType>.dialogSequenceSpawner.Despawn(this);

            callback?.Invoke(this);
        }

        private IEnumerator HandleCurrentEntry(DialogContext dialogContext, BaseEntry baseEntry) {
            if (baseEntry is DialogEntry dialogEntry) {
                yield return this.chatBubble.PopulateChatBubble(dialogContext, dialogEntry);
            }
            else {
                Debug.LogError($"Unknown entry type {baseEntry}");
            }

            // Be finished with the entry
            dialogContext.TryInvoke(baseEntry);
        }

        internal void HandleTypewriterEvent(BaseEntry entry, ITypewriterContext iContext) {
            if (iContext is not DialogContext dialogContext) {
                Debug.LogError($"Unknown context type {iContext}");
                return;
            }

            if (entry is RuleEntry ruleEntry) {
                Debug.Log($"@Sequence.HandleTypewriterEvent.rule; {ruleEntry}");
                // TODO: Check if this rule belongs to the currently held rule
                this.currentEntry = ruleEntry;
                return;
            }

            Debug.LogError($"Unknown entry type {entry}");
        }

        //public override bool HasAnotherEvent() {
        //    if (this.dialogEventHandler.entry == null) {
        //        return false;
        //    }

        //    return this.GetContext().WouldInvoke(this.dialogEventHandler.entry);
        //}


        public override EventPriority GetCurrentEventPriority() {
			throw new System.NotImplementedException();
		}

		public override IEnumerator OnCancel() {
			this.isFinished = true;
            DialogManagerBase<SpeakerType>.dialogSequenceSpawner.Despawn(this);
			yield break;
        }







		// Chat bubble functions; override these to do things like a chat bubble history
        protected virtual void CreateNewChatBubble() {
            this.chatBubble = DialogManagerBase<SpeakerType>.chatBubbleSpawner.Spawn();
            this.chatBubble.Hide();
        }

        protected virtual void DoneWithChatBubble() {
            TypewriterDatabase.Instance.RemoveListener(this.HandleTypewriterEvent);
            DialogManagerBase<SpeakerType>.chatBubbleSpawner.Despawn(this.chatBubble);
        }






        //    public override IEnumerator Start(IContext iContext) {

        //        if (iContext is not DialogContext dialogContext) {
        //            Debug.LogError($"Unknown context type {iContext}");
        //            yield break;
        //        }

        //        if (entry is EventEntry eventEntry) {
        //            if (!dialogContext.WouldInvoke(entry)) {
        //                return;
        //            }

        //            if (this.currentSequence != null) {
        //                // Check if the priority of this event is bigger than the current event
        //                return;

        //                //this.currentSequence.Cancel();
        //            }

        //            this.currentSequence = this.SpawnDialogueSequence();
        //            this.currentSequence.SetEvent(eventEntry, this.SpawnChatBubble());
        //            dialogContext.Process(entry);
        //            return;
        //        }

        //        if (entry is DialogueEntry dialogueEntry) {
        //            this.currentSequence.Begin(context, dialogueEntry);
        //            return;
        //        }

        //        Debug.LogError($"Unknown entry type {entry}");


        //        throw new System.Exception("Unknown entry type for '" + entry + "'");
        //    }
        //}






        //internal void SetEvent(EventEntry entry, ChatBubbleBase chatBubble) {
        //    this.currentEventEntry = entry;
        //    this.currentType = SequenceType.Unknown;
        //    this.currentChatBubble = chatBubble;
        //}

        //internal void Begin(Context context, DialogueEntry entry) {
        //    this.currentRuleEntry = entry;
        //    this.currentContext = context;
        //    this.currentType = SequenceType.Dialogue;
        //    this.currentSpeaker = EventSequencer.instance.LookupSpeaker(entry);

        //    this.currentChatBubble.Begin(entry.Text, this.currentSpeaker);
        //}

        //internal void Cancel() {
        //    this.currentType = SequenceType.Canceled;

        //    this.inputMapper.EventInteract.RemoveListener(this.Cancel);
        //}

        //internal void Finish() {
        //    this.inputMapper.EventInteract.AddListener(this.NextDialogue);
        //}

        //internal void NextDialogue() {
        //    this.inputMapper.EventInteract.RemoveListener(this.NextDialogue);
        //}

        //internal void NextDialogueArrayLine() {
        //    if (this.nextDialogueArrayIndex + 1 > this.latestDialogueArrayEntry.Lines.List.Length) {
        //        this.currentChatBubble.End();
        //        return;
        //    }

        //    DialogueArrayEntryLine line = this.latestDialogueArrayEntry.Lines.List[this.nextDialogueArrayIndex];
        //    this.currentSpeaker = EventSequencer.instance.LookupSpeaker(line);
        //    this.currentChatBubble.EventEnded.AddListener(NextDialogueArrayLine);
        //    this.currentChatBubble.Begin(line.Text, this.currentSpeaker);

        //    this.nextDialogueArrayIndex++;
        //}
    }
}