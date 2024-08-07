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
	public class HistoryChatSequence<SpeakerType, EmotionType, ActionType> : BaseChatSequence<SpeakerType, EmotionType, ActionType> where SpeakerType : Enum where EmotionType : Enum where ActionType : Enum {
		[Readonly][SerializeField] protected CodeSpawner<HistoryChatSequence<SpeakerType, EmotionType, ActionType>> spawner;
		public override void OnSpawn(object spawner) {
			base.OnSpawn(spawner);

			if (spawner is CodeSpawner<HistoryChatSequence<SpeakerType, EmotionType, ActionType>> sequenceSpawner) {
				this.spawner = sequenceSpawner;
			}
			else {
				Debug.LogError($"Unknown spawner type {spawner}");
			}
		}

		public override void OnDespawn(object spawner) {
			base.OnDespawn(spawner);
			this.spawner = null;
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
			HistoryDialogManager<SpeakerType, EmotionType, ActionType>.instance.EventUserInteractedWithDialog.AddListener(this.OnUserInteracted);
			HistoryDialogManager<SpeakerType, EmotionType, ActionType>.instanceHistory.ShowChat();
			yield return null;
		}

		public override IEnumerator Start_Post(DialogContext dialogContext) {
			HistoryDialogManager<SpeakerType, EmotionType, ActionType>.instanceHistory.HideChat();
			yield return null;
		}

		protected override IEnumerator HandleCurrentEntry(DialogContext dialogContext, BaseEntry baseEntry) {
			if (this.chatBubble != null) {
				this.chatBubble.SetOpacity(0.6f);
			}

			this.chatBubble = HistoryDialogManager<SpeakerType, EmotionType, ActionType>.chatBubbleSpawner.Spawn();

			yield return base.HandleCurrentEntry(dialogContext, baseEntry);
		}

		public override IEnumerator OnCancel() {
			this.Despawn();
			yield return base.OnCancel();
		}
	}
}