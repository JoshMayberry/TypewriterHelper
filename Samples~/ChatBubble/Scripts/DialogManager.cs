using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using jmayberry.Spawner;
using jmayberry.TypewriterHelper;

namespace jmayberry.TypewriterHelper.Samples.ChatBubble {
	public enum MySpeakerType {
		Unknown,
		System,
		Slime,
		Skeleton,
	}

	public class DialogManager : ObjectDialogManager<MySpeakerType> {
		private void Update() {
			if (Input.GetKeyDown("space")) {
				this.EventUserInteractedWithDialog.Invoke();
			}
		}
	}
}