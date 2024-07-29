using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using jmayberry.Spawner;
using jmayberry.TypewriterHelper;

namespace jmayberry.TypewriterHelper.Samples.ChatBubble {
	[RequireComponent(typeof(AudioSource))]
	public class DialogManager : ObjectDialogManager<MySpeakerType, MyEmotionType, MyActionType> {
		public AudioSource audioSource;

		public static DialogManager myInstance { get; private set; }

		protected override void Awake() {
			base.Awake();

			if (myInstance != null && myInstance != this) {
				Debug.LogError("Found more than one DialogManager in the scene.");
				Destroy(this.gameObject);
				return;
			}

			myInstance = this;

			this.audioSource = GetComponent<AudioSource>();

		}

		private void Update() {
			if (Input.GetKeyDown("space")) {
				this.EventUserInteractedWithDialog.Invoke();
			}
		}
	}
}