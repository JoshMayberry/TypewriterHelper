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

namespace jmayberry.TypewriterHelper.Samples.ChatPortrait {
	public enum MySpeakerType {
		Unknown,
		System,
		Slime,
		Skeleton,
	}

    public enum MyEmotionType {
        Normal,
        Happy,
        Angry,
        Sad,
    }

    public class DialogManager : PortraitDialogManager<MySpeakerType, MyEmotionType> {
        private void Update() {
			if (Input.GetKeyDown("space")) {
				this.EventUserInteractedWithDialog.Invoke();
			}
		}
	}
}