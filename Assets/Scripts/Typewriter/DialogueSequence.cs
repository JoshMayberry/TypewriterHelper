using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Aarthificial.Typewriter.References;
using Aarthificial.Typewriter.Tools;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.Editor.Common;

namespace Aarthificial.Typewriter.Applications {
    public enum SequenceType {
        Unknown,
        Canceled,
        Completed,
        Dialogue,
        DialogueArray
    }

    public class DialogueSequence {
        private Speaker currentSpeaker;
        private Context currentContext;
        private EventEntry currentEvent;

        private RuleEntry currentRule;
        private DialogueArrayEntry latestDialogueArrayEntry;

        [Readonly] public SequenceType currentType;
        [Readonly] [SerializeField] private ChatBubble currentChatBubble;
        [Readonly] [SerializeField] private int nextDialogueArrayIndex;

        internal DialogueSequence() {
            this.currentType = SequenceType.Unknown;
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

            InputManager.instance.EventInteract.RemoveListener(this.Cancel);
        }

        internal void Finish() {
            InputManager.instance.EventInteract.AddListener(this.NextDialogue);
        }

        internal void NextDialogue() {
            InputManager.instance.EventInteract.RemoveListener(this.NextDialogue);
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
    }
}