using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using jmayberry.Spawner;
using jmayberry.TypewriterHelper;

namespace jmayberry.TypewriterHelper.Samples.ChatHistory {
	public enum MySpeakerType {
		Unknown,
		System,
		Slime,
		Skeleton,
	}

	public class DialogManager : DialogManagerBase<MySpeakerType> {
		protected internal static CodeSpawner<PermanentChatSequence<MySpeakerType>> dialogSequenceSpawner;

		protected override void Awake() {
			base.Awake();
			dialogSequenceSpawner = new CodeSpawner<PermanentChatSequence<MySpeakerType>>();
		}
		protected override BaseChatSequence<MySpeakerType> SpawnDialogSequence() {
			return dialogSequenceSpawner.Spawn();
		}

		protected override void DespawnDialogSequence(BaseChatSequence<MySpeakerType> spawnling) {
			dialogSequenceSpawner.Despawn((PermanentChatSequence<MySpeakerType>)spawnling);
		}

		private void Update() {
			if (Input.GetKeyDown("space")) {
				this.EventUserInteractedWithDialog.Invoke();
			}
		}
	}
}