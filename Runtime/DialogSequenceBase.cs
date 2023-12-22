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
    public class DialogSequenceBase : EventSequenceBase {
        [Header("Typewriter")]
        public EventEntry rootEventEntry;
        [Readonly] public ISpeaker currentSpeaker;

        [Required] public DialogContext defaultContext;
        [Readonly] public DialogContext currentContext;
        [Readonly] public ChatBubbleBase currentChatBubble;
        [Readonly] public DialogEventHandler dialogEventHandler;

        public DialogSequenceBase() : base() {
            this.dialogEventHandler = new DialogEventHandler();
        }

        public DialogSequenceBase(EventEntry eventEntry) : base() {
            this.dialogEventHandler = new DialogEventHandler();
            this.SetEventEntry(eventEntry);
        }

        public void SetEventEntry(EventEntry eventEntry) {
            this.rootEventEntry = eventEntry;
            this.dialogEventHandler.entry = this.rootEventEntry;
        }

        public DialogContext GetContext() {
            if (this.currentContext != null) {
                return this.currentContext;
            }

            return this.defaultContext;
        }

        public override bool HasAnotherEvent() {
            if (this.dialogEventHandler.entry == null) {
                return false;
            }

            return this.GetContext().WouldInvoke(this.dialogEventHandler.entry);
        }

        public override EventPriority GetCurrentEventPriority() {
            throw new System.NotImplementedException();
        }

        public override IEnumerator Start(IContext iContext) {
            if (this.rootEventEntry == null) {
                Debug.LogError("Root Entry not yet set");
                yield break;
            }

            yield return base.Start(iContext);
        }

        // These functions are here to make the application of the EventSequencer package work
        public override EventBase GetNextEvent() {
            return this.dialogEventHandler;
        }

        public override void AddEvent(params EventBase[] events) {
            Debug.LogWarning("AddEvent is unused");
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