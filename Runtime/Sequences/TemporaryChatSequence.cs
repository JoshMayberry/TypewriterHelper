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
	public class TemporaryChatSequence<SpeakerType> : BaseChatSequence<SpeakerType> where SpeakerType : Enum {

		public override IEnumerator Start_Pre(DialogContext dialogContext) {
			this.CreateNewChatBubble();
			yield return null;
		}

		public override IEnumerator Start_Post(DialogContext dialogContext) {
			DialogManagerBase<SpeakerType>.instance.DespawnDialogSequence(this);
			yield return null;
		}

		protected virtual void CreateNewChatBubble() {
			DialogManagerBase<SpeakerType>.instance.EventUserInteractedWithDialog.AddListener(this.OnUserInteracted);
			this.chatBubble = DialogManagerBase<SpeakerType>.chatBubbleSpawner.Spawn();
		}

		protected virtual void DoneWithChatBubble() {
			
		}
	}
}