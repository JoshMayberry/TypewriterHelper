using UnityEngine;
using Aarthificial.Typewriter.References;

namespace jmayberry.TypewriterHelper.Applications {
        public enum SpeakerType {
		Skeleton = 0,
		Slime = 1,
		System = 2
	}

	public class Speaker : MonoBehaviour {
		[Header("Speaker")]
		[Required][SerializeField] internal Transform chatBubblePosition;
		[Required][SerializeField] internal ChatBubbleAlignment chatBubbleAlignment = ChatBubbleAlignment.TopMiddle;
		[Required][SerializeField] internal SpeakerType speakerType = SpeakerType.Skeleton;

		[SerializeField] private EntryReference speakerReference;

		[SerializeField] internal string displayName;

		internal Context typewriterContext = new Context();

		void OnEnable() {
			if (this.speakerReference == 0) {
				return;
			}
			EventSequencer.instance.speakerLookup.Add(this.speakerReference.ID, this);
		}

		void OnDisable() {
			if (this.speakerReference == 0) {
				return;
			}
			EventSequencer.instance.speakerLookup.Remove(this.speakerReference.ID);
		}
	}
}
