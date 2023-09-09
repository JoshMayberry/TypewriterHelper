using UnityEngine;
using UnityEngine.Events;
using Aarthificial.Typewriter.References;
using Aarthificial.Typewriter.Entries;
using jmayberry.TypewriterHelper.Entries;

namespace jmayberry.TypewriterHelper.Applications {
        public enum SequenceType {
        Unknown,
        Canceled,
        Completed,
        Dialogue,
        DialogueArray
    }

    public abstract class DialogueSequenceBase {
        private Speaker currentSpeaker;
        private Context currentContext;
        private EventEntry currentEvent;

        private RuleEntry currentRule;
        private DialogueArrayEntry latestDialogueArrayEntry;

        private UnityEvent _EventInteract;

        [Readonly] public SequenceType currentType;
        [Readonly] [SerializeField] private ChatBubble currentChatBubble;
        [Readonly] [SerializeField] private int nextDialogueArrayIndex;

        internal DialogueSequenceBase() {
            this.currentType = SequenceType.Unknown;
            this._EventInteract = this.GetEventInteract();
        }

        internal void SetEvent(EventEntry entry, ChatBubble chatBubble) {
            this.currentEvent = entry;
            this.currentType = SequenceType.Unknown;
            this.currentChatBubble = chatBubble;
        }

        internal void Begin(Context context, DialogueEntry entry) {
            this.currentRule = entry;
            this.currentContext = context;
            this.currentType = SequenceType.Dialogue;
            this.currentSpeaker = EventSequencer.instance.LookupSpeaker(entry);

            this.currentChatBubble.Begin(entry.Text, this.currentSpeaker);
        }

        internal void Begin(Context context, DialogueArrayEntry entry) {
            this.currentRule = entry;
            this.currentContext = context;
            this.latestDialogueArrayEntry = entry;
            this.currentType = SequenceType.DialogueArray;
            this.currentSpeaker = null;
            this.nextDialogueArrayIndex = 0;

            this.NextDialogueArrayLine();
        }

        internal void Cancel() {
            this.currentType = SequenceType.Canceled;

            this._EventInteract.RemoveListener(this.Cancel);
        }

        internal void Finish() {
            this._EventInteract.AddListener(this.NextDialogue);
        }

        internal void NextDialogue() {
            this._EventInteract.RemoveListener(this.NextDialogue);
        }

        internal void NextDialogueArrayLine() {
            if (this.nextDialogueArrayIndex + 1 > this.latestDialogueArrayEntry.Lines.List.Length) {
                this.currentChatBubble.End();
                return;
            }

            DialogueArrayEntryLine line = this.latestDialogueArrayEntry.Lines.List[this.nextDialogueArrayIndex];
            this.currentSpeaker = EventSequencer.instance.LookupSpeaker(line);
            this.currentChatBubble.EventEnded.AddListener(NextDialogueArrayLine);
            this.currentChatBubble.Begin(line.Text, this.currentSpeaker);

            this.nextDialogueArrayIndex++;
        }

        internal abstract UnityEvent GetEventInteract();
    }
}