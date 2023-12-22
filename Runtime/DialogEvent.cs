using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using jmayberry.EventSequencer;
using jmayberry.CustomAttributes;
using jmayberry.TypewriterHelper.Entries;

using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter;

namespace jmayberry.TypewriterHelper {
	public class DialogEventHandler : EventBase {
		[Readonly] public EventEntry spawnEvent;
        [Readonly] public List<RuleEntry> dialogPieces;

		public DialogEventHandler() {
            this.dialogPieces = new List<RuleEntry>();
        }

		public override IEnumerator OnExecute(IContext iContext) {
            if (iContext is not DialogContext dialogContext) {
                Debug.LogError($"Unknown context type {iContext}");
                yield break;
            }

            Debug.Log($"@OnExecute; {this.entry}");

            yield return new WaitForSeconds(3);

            dialogContext.Process(this.entry);

            this.entry = null;

        }

        internal void HandleTypewriterEvent(BaseEntry entry, ITypewriterContext iContext) {



            Debug.Log($"@HandleTypewriterEvent.Rule; {triggerEntry}");
            dialogContext.TryInvoke(triggerEntry);

        }
    }
}
