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
    [Serializable]
    public class ObjectChatBubbleInfo : BaseChatBubbleInfo {
        [Required] public Sprite fallbackPointToSpeakerSprite;
        [SerializedDictionary("Chat Type", "Pointer")] public SerializedDictionary<ChatBubbleType, Sprite> pointToSpeakerSprite;
    }

    public class ObjectDialogManager<SpeakerType> : BaseDialogManager<SpeakerType> where SpeakerType : Enum {
        [Header("Object: Setup")]
		[Required][SerializeField] protected BaseChat<SpeakerType> chatBubblePrefab;
        [SerializedDictionary("Speaker Type", "Chat Bubble")] public SerializedDictionary<SpeakerType, ObjectChatBubbleInfo> chatBubbleInfo = new SerializedDictionary<SpeakerType, ObjectChatBubbleInfo>();
        public ObjectChatBubbleInfo fallbackChatBubbleInfo;

        protected internal static CodeSpawner<ObjectChatSequence<SpeakerType>> dialogSequenceSpawner;

        public static ObjectDialogManager<SpeakerType> instanceObject { get; private set; }

        protected override void Awake() {
            base.Awake();

            if (instanceObject != null && instanceObject != this) {
                Debug.LogError("Found more than one ObjectDialogManager<SpeakerType> in the scene.");
                Destroy(this.gameObject);
                return;
            }

            instanceObject = this;

			dialogSequenceSpawner = new CodeSpawner<ObjectChatSequence<SpeakerType>>();
			chatBubbleSpawner = new UnitySpawner<BaseChat<SpeakerType>>(chatBubblePrefab);
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