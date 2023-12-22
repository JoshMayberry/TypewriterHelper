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

		[Readonly] public BaseEntry entry;

		public DialogEventHandler() {}

		public override IEnumerator OnExecute(IContext iContext) {
			if (iContext is not DialogContext dialogContext) {
				Debug.LogError($"Unknown context type {iContext}");
				yield break;
			}

			if (this.entry is FactEntry decisionEntry) {
				dialogContext.TryInvoke(decisionEntry);
				yield break;
			}

			if (this.entry is RuleEntry triggerEntry) {
				dialogContext.TryInvoke(triggerEntry);
				yield break;
			}

			if (this.entry is EventEntry eventEntry) {
				if (!dialogContext.WouldInvoke(this.entry)) {
					yield break;
				}

				dialogContext.Process(this.entry);
				yield break;
            }

			Debug.LogError($"Unknown entry type {entry}");

			throw new System.Exception("Unknown entry type for '" + entry + "'");
		}
	}
}
