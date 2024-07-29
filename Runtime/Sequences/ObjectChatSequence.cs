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
	public class ObjectChatSequence<SpeakerType, EmotionType, ActionType> : BaseChatSequence<SpeakerType, EmotionType, ActionType> where SpeakerType : Enum where EmotionType : Enum where ActionType : Enum {
		[Readonly][SerializeField] protected CodeSpawner<ObjectChatSequence<SpeakerType, EmotionType, ActionType>> spawner;
		public override void OnSpawn(object spawner) {
			base.OnSpawn(spawner);

			if (spawner is CodeSpawner<ObjectChatSequence<SpeakerType, EmotionType, ActionType>> sequenceSpawner) {
				this.spawner = sequenceSpawner;
			}
			else {
				Debug.LogError($"Unknown spawner type {spawner}");
			}
		}

		public override void OnDespawn(object spawner) {
			base.OnDespawn(spawner);
			this.spawner = null;

			if (this.chatBubble != null) {
				BaseDialogManager<SpeakerType, EmotionType, ActionType>.instance.StartCoroutine(this.chatBubble.DespawnCoroutine());
				this.chatBubble = null;
			}
		}

		public virtual void Despawn() {
			if (this.spawner != null) {
				this.spawner.Despawn(this);
			}
			else {
				Debug.LogError("Spawner not set");
			}
		}

		public override IEnumerator Start_Pre(DialogContext dialogContext) {
			ObjectDialogManager<SpeakerType, EmotionType, ActionType>.instance.EventUserInteractedWithDialog.AddListener(this.OnUserInteracted);
			this.chatBubble = ObjectDialogManager<SpeakerType, EmotionType, ActionType>.chatBubbleSpawner.Spawn();
			yield return null;
		}

		public override IEnumerator Start_Post(DialogContext dialogContext) {
			yield return this.chatBubble.OnFinishedSequence();
		}

		public override IEnumerator OnCancel() {
			this.Despawn();
			yield return base.OnCancel();
		}
	}
}