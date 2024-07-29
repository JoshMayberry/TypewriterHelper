using System;
using System.Collections.Generic;
using UnityEngine;

using AYellowpaper.SerializedCollections;
using Aarthificial.Typewriter.References;
using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.Tools;
using Aarthificial.Typewriter;

using jmayberry.TypewriterHelper.Entries;
using jmayberry.CustomAttributes;
using jmayberry.EventSequencer;
using jmayberry.Spawner;
using UnityEngine.Events;

namespace jmayberry.TypewriterHelper.Samples.ChatHistory {
	[RequireComponent(typeof(AudioSource))]
	public class DialogManager : HistoryDialogManager<MySpeakerType, MyEmotionType, MyActionType> {
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