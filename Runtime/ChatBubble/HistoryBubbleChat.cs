using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

using jmayberry.CustomAttributes;
using jmayberry.Spawner;
using jmayberry.TypewriterHelper.Entries;
using Aarthificial.Typewriter;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using Unity.Plastic.Newtonsoft.Json.Linq;

namespace jmayberry.TypewriterHelper {
	[Serializable]
	public abstract class HistoryBubbleChat<SpeakerType> : BaseUiChat<SpeakerType> where SpeakerType : Enum {
		[Readonly]
		[SerializeField] protected List<BaseChat<SpeakerType>> subBubbleList;
		[SerializeField] protected BaseChat<SpeakerType> subBubbleLatest;

		protected void Awake() {
			this.subBubbleList = new List<BaseChat<SpeakerType>>();
		}

		protected virtual void Clear() {
			foreach (var subBubble in this.subBubbleList) {
				subBubble.Despawn();
			}

			this.subBubbleList.Clear();
		}

		public override void SoftReset(Transform newPosition = null) {
			this.Clear();
			base.SoftReset(newPosition);
		}

		public override void OnSpawn(object spawner) {
			base.OnSpawn(spawner);
			this.transform.SetParent(HistoryDialogManager<SpeakerType>.instanceHistory.chatBubbleContainer.transform);
		}

		public override void OnDespawn(object spawner) {
			base.OnDespawn(spawner);
			this.transform.SetParent(HistoryDialogManager<SpeakerType>.instanceHistory.transform);
			this.Clear();
		}

		public override IEnumerator Populate_Loop(DialogContext dialogContext, DialogEntry dialogEntry, string dialog, int i, bool isLastInLoop, bool isLastInSequence) {
			if (i == 0) {
				yield return base.Populate_Loop(dialogContext, dialogEntry, dialog, i, isLastInLoop, isLastInSequence);
				yield break;
			}

		  
			this.SetOpacity(0.6f);

			this.subBubbleLatest = this.spawner.Spawn();
			this.subBubbleList.Add(this.subBubbleLatest);

			yield return this.subBubbleLatest.Populate_PreLoop(dialogContext, dialogEntry);
			yield return this.subBubbleLatest.Populate_Loop(dialogContext, dialogEntry, dialog, 0, isLastInLoop, isLastInSequence);
		}

		public override void SetOpacity(float value) {
			base.SetOpacity(value);

			if (this.subBubbleLatest != null) {
                this.subBubbleLatest.SetOpacity(value);
			}
		}

		public override void OnSkipToEnd() {
			base.OnSkipToEnd();

            if (this.subBubbleLatest != null) {
				if (this.subBubbleLatest == this) {
					Debug.LogError("Something wrong happened...");
					return;
				}

				this.subBubbleLatest.OnSkipToEnd();
			}
		}
	}
}