using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using jmayberry.TypewriterHelper;

namespace jmayberry.TypewriterHelper.Samples.ChatBubble {
	public class ChatBubble : PointerBubbleChat<MySpeakerType, MyEmotionType, MyActionType> {
		protected override void UpdateTextProgress_PlaySound() {
			if (this.currentVoice == null) {
				return;
			}

			if (this.currentVoice is UnitySpeakerVoice unityVoice) {
				unityVoice.PlaySound(DialogManager.myInstance.audioSource, this.currentTextLength, this.UpdateTextProgress_GetCurrentCharacter());
			}
		}
	}
}
