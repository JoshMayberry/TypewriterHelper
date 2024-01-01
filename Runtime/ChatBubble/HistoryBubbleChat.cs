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

namespace jmayberry.TypewriterHelper {
	[Serializable]
	public abstract class HistoryBubbleChat<SpeakerType> : BaseUiChat<SpeakerType> where SpeakerType : Enum {
		public override void OnSpawn(object spawner) {
			base.OnSpawn(spawner);
			this.transform.SetParent(HistoryDialogManager<SpeakerType>.instanceHistory.chatBubbleContainer.transform);
		}

        public override void OnDespawn(object spawner) {
            base.OnDespawn(spawner);
            this.transform.SetParent(HistoryDialogManager<SpeakerType>.instanceHistory.transform);
        }
    }
}