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

		public override void OnDespawn(object spawner) {
			throw new System.NotImplementedException();
		}

		public override void OnSpawn(object spawner) {
			throw new System.NotImplementedException();
		}

		protected internal override IEnumerator DespawnCoroutine() {
			throw new System.NotImplementedException();
		}

		protected internal override IEnumerator OnFinishedSequence() {
			throw new System.NotImplementedException();
		}

		protected override IEnumerator Populate_PreLoop() {
			throw new System.NotImplementedException();
		}

		protected override IEnumerator Populate_PrepareText(string dialog) {
			throw new System.NotImplementedException();
		}

		protected override void UpdateSpeaker_setText(Speaker<SpeakerType> newSpeaker) {
			throw new System.NotImplementedException();
		}

		protected override bool UpdateSprites() {
			throw new System.NotImplementedException();
		}

		protected override void UpdateTextProgress_SetText(int textLength) {
			throw new System.NotImplementedException();
		}
	}
}