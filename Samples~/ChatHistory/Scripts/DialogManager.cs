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
	public enum MySpeakerType {
		Unknown,
		System,
		Slime,
		Skeleton,
	}

	public class DialogManager : HistoryDialogManager<MySpeakerType> {
        private void Update() {
			if (Input.GetKeyDown("space")) {
				this.EventUserInteractedWithDialog.Invoke();
			}
		}
	}
}