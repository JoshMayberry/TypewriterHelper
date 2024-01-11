using System;
using UnityEngine;
using UnityEngine.Assertions;

using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.References;

using jmayberry.EventSequencer;

namespace jmayberry.TypewriterHelper.Entries {
	[Serializable]
	public class BaseDialogEntry<EmotionType> : RuleEntry where EmotionType : Enum {
		public EventPriority priority = EventPriority.None;
		public ChatBubbleType chatKind = ChatBubbleType.Normal;
		public EmotionType emotion;
		[EntryFilter(BaseType = typeof(SpeakerEntry))] [SerializeField] private EntryReference _speaker;
		[SerializeField] public BaseSpeakerVoice speakerVoiceOverride;

		[SerializeField][InspectorName("Dialog")][TextArea] public string[] TextList = { "" };

		//public bool IsChoice; // TODO: Impliment this as a list of possible choices and somehow pipe them in
		[Range(0.25f, 2)] public float Speed = 1f;
		public PortraitSide portraitSide;

		public SpeakerEntry Speaker {
			get {
				var speaker = _speaker.GetEntry<SpeakerEntry>();
				Assert.IsNotNull(
				  speaker,
				  $"Invalid speaker ID ({_speaker.ID}) required by \"{this}\""
				);
				return speaker;
			}
		}

	}
}