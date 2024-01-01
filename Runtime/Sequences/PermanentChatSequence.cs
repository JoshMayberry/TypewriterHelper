using System;
using System.Linq;
using System.Collections;

using UnityEngine;
using UnityEngine.Events;

using Aarthificial.Typewriter;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.References;

using jmayberry.TypewriterHelper.Entries;
using jmayberry.CustomAttributes;
using jmayberry.EventSequencer;
using jmayberry.Spawner;

namespace jmayberry.TypewriterHelper {
	public class PermanentChatSequence<SpeakerType> : BaseChatSequence<SpeakerType> where SpeakerType : Enum {
		public override IEnumerator Start_Post(DialogContext dialogContext) {
			throw new NotImplementedException();
		}

		public override IEnumerator Start_Pre(DialogContext dialogContext) {
			throw new NotImplementedException();
		}
	}
}