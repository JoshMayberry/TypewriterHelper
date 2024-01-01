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
	public class ObjectChatSequence<SpeakerType> : BaseChatSequence<SpeakerType> where SpeakerType : Enum {
		public override IEnumerator Start_Pre(DialogContext dialogContext) {
			ObjectDialogManager<SpeakerType>.instance.EventUserInteractedWithDialog.AddListener(this.OnUserInteracted);
			this.chatBubble = ObjectDialogManager<SpeakerType>.chatBubbleSpawner.Spawn();
			yield return null;
		}

		public override IEnumerator Start_Post(DialogContext dialogContext) {
			yield return this.chatBubble.OnFinishedSequence();
		}

		public override IEnumerator OnCancel() {
			ObjectDialogManager<SpeakerType>.dialogSequenceSpawner.Despawn(this);
			yield return base.OnCancel();
		}

		public override void OnDespawn(object spawner) {
			base.OnDespawn(spawner);

			if (this.chatBubble != null) {
				BaseDialogManager<SpeakerType>.instance.StartCoroutine(this.chatBubble.DespawnCoroutine());
				this.chatBubble = null;
			}
		}
	}
}