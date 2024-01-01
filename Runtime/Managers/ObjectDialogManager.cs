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

namespace jmayberry.TypewriterHelper {
	public class ObjectDialogManager<SpeakerType> : BaseDialogManager<SpeakerType> where SpeakerType : Enum {
		protected internal static CodeSpawner<ObjectChatSequence<SpeakerType>> dialogSequenceSpawner;

		protected override void Awake() {
			base.Awake();
			dialogSequenceSpawner = new CodeSpawner<ObjectChatSequence<SpeakerType>>();
		}

		protected override BaseChatSequence<SpeakerType> SpawnDialogSequence() {
			return dialogSequenceSpawner.Spawn();
        }

        protected override void OnSequenceFinished(SequenceBase sequence) {
            base.OnSequenceFinished(sequence);

			if (sequence is ObjectChatSequence<SpeakerType> objectSequence) {
                objectSequence.Despawn();
			}
        }
    }
}