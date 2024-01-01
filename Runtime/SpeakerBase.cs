using System;
using UnityEngine;

using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.References;
using Aarthificial.Typewriter.Entries;

using jmayberry.CustomAttributes;
using System.Collections.Generic;

namespace jmayberry.TypewriterHelper {
	public class Speaker<SpeakerType> : MonoBehaviour where SpeakerType : Enum {
		[Header("Setup")]
		[Required][SerializeField] internal Transform chatBubblePosition;
		[Required][SerializeField] internal ChatBubbleAlignment chatBubbleAlignment = ChatBubbleAlignment.TopMiddle;
		[Required][SerializeField] internal SpeakerType speakerType;
		[SerializeField] internal string displayName;
		[EntryFilter(Variant = EntryVariant.Fact, AllowEmpty = true)] [SerializeField] private List<EntryReference> speakerReference;

		internal DialogContext typewriterContext = new DialogContext();

		void OnEnable() {
			foreach (EntryReference reference in speakerReference) {
				if (reference == 0) {
					continue;
				}
				BaseDialogManager<SpeakerType>.speakerLookup.Add(reference.ID, this);
			}
		}

		void OnDisable() {
			foreach (EntryReference reference in speakerReference) {
				if (reference == 0) {
					continue;
				}
				BaseDialogManager<SpeakerType>.speakerLookup.Remove(reference.ID);
			}
		}

		internal bool IsDifferent(Speaker<SpeakerType> newSpeaker) {
			if (newSpeaker == null) {
				return true;
			}

			return !newSpeaker.speakerType.Equals(this.speakerType);
		}
	}
}
