using System;
using UnityEngine;

using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.References;
using Aarthificial.Typewriter.Entries;

using jmayberry.CustomAttributes;

namespace jmayberry.TypewriterHelper {
	public class Speaker<SpeakerType> : MonoBehaviour where SpeakerType : Enum {
		[Header("Setup")]
		[Required][SerializeField] internal Transform chatBubblePosition;
		[Required][SerializeField] internal ChatBubbleAlignment chatBubbleAlignment = ChatBubbleAlignment.TopMiddle;
		[Required][SerializeField] internal SpeakerType speakerType;
        [SerializeField] internal string displayName;

        [Header("Debug")]
        [EntryFilter(Variant = EntryVariant.Fact, AllowEmpty = true)] [SerializeField] private EntryReference speakerReference;

		internal DialogContext typewriterContext = new DialogContext();

		void OnEnable() {
			if (this.speakerReference == 0) {
				return;
			}

            DialogManagerBase<SpeakerType>.speakerLookup.Add(this.speakerReference.ID, this);
		}

		void OnDisable() {
			if (this.speakerReference == 0) {
				return;
			}

			DialogManagerBase<SpeakerType>.speakerLookup.Remove(this.speakerReference.ID);
        }

        internal bool IsDifferent(Speaker<SpeakerType> newSpeaker) {
			if (newSpeaker == null) {
				return true;
			}

            return !newSpeaker.speakerType.Equals(this.speakerType);
        }
    }
}
