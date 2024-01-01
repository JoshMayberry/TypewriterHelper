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
	public class HistoryChatSequence<SpeakerType> : BaseChatSequence<SpeakerType> where SpeakerType : Enum {
		public override IEnumerator Start_Pre(DialogContext dialogContext) {
			HistoryDialogManager<SpeakerType>.instance.EventUserInteractedWithDialog.AddListener(this.OnUserInteracted);
			yield return null;
		}

		public override IEnumerator Start_Post(DialogContext dialogContext) {
			yield return null;
		}

		protected override IEnumerator HandleCurrentEntry(DialogContext dialogContext, BaseEntry baseEntry) {
			if (this.chatBubble != null) {
				this.chatBubble.SetOpacity(0.6f);
				this.chatBubble = null;
			}

			this.chatBubble = HistoryDialogManager<SpeakerType>.chatBubbleSpawner.Spawn();

			yield return base.HandleCurrentEntry(dialogContext, baseEntry);
		}

		public override IEnumerator OnCancel() {
			HistoryDialogManager<SpeakerType>.dialogSequenceSpawner.Despawn(this);
			yield return base.OnCancel();
		}
	}
}