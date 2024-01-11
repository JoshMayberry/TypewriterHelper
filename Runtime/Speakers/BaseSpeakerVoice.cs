using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jmayberry.TypewriterHelper {
	public abstract class BaseSpeakerVoice : ScriptableObject {
		public string id;

		public abstract void NewConversation();

		public abstract void PlaySound(int currentDisplayedCharacterCount, char currentCharacter);

		public abstract void StopSound();
	}
}