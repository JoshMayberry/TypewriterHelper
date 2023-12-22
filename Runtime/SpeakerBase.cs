using UnityEngine;
using Aarthificial.Typewriter.References;
using jmayberry.CustomAttributes;
using System;

namespace jmayberry.TypewriterHelper {

	public interface ISpeaker {

	}



	public class Speaker<SpeakerType> : MonoBehaviour, ISpeaker where SpeakerType : Enum {
		[Header("Speaker")]
		[Required][SerializeField] internal Transform chatBubblePosition;
		[Required][SerializeField] internal ChatBubbleAlignment chatBubbleAlignment = ChatBubbleAlignment.TopMiddle;
		[Required][SerializeField] internal SpeakerType speakerType;

		[SerializeField] private EntryReference speakerReference;

		[SerializeField] internal string displayName;

		internal Context typewriterContext = new Context();

		void OnEnable() {
			if (this.speakerReference == 0) {
				return;
			}
			DialogManager<SpeakerType>.instance.speakerLookup.Add(this.speakerReference.ID, this);
		}

		void OnDisable() {
			if (this.speakerReference == 0) {
				return;
			}
			DialogManager<SpeakerType>.instance.speakerLookup.Remove(this.speakerReference.ID);
		}
	}
}
