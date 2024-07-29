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
	public class ObjectChatBubbleInfo<EmotionType> : BaseChatBubbleInfo<EmotionType> where EmotionType : Enum {
		[Required] public Sprite fallbackPointToSpeakerSprite;
		[SerializedDictionary("Chat Type", "Pointer")] public SerializedDictionary<ChatBubbleType, Sprite> pointToSpeakerSprite;
	}

	public class ObjectDialogManager<SpeakerType, EmotionType, ActionType> : BaseDialogManager<SpeakerType, EmotionType, ActionType> where SpeakerType : Enum where EmotionType : Enum where ActionType : Enum {
		[Header("Object: Setup")]
		[Required][SerializeField] protected BaseChat<SpeakerType, EmotionType, ActionType> chatBubblePrefab;
		[SerializedDictionary("Speaker Type", "Chat Bubble")] public SerializedDictionary<SpeakerType, ObjectChatBubbleInfo<EmotionType>> chatBubbleInfo = new SerializedDictionary<SpeakerType, ObjectChatBubbleInfo<EmotionType>>();
		public ObjectChatBubbleInfo<EmotionType> fallbackChatBubbleInfo;

		protected internal static CodeSpawner<ObjectChatSequence<SpeakerType, EmotionType, ActionType>> dialogSequenceSpawner;

		public static ObjectDialogManager<SpeakerType, EmotionType, ActionType> instanceObject { get; private set; }

		protected override void Awake() {
			base.Awake();

			if (instanceObject != null && instanceObject != this) {
				Debug.LogError("Found more than one ObjectDialogManager<SpeakerType, EmotionType, ActionType> in the scene.");
				Destroy(this.gameObject);
				return;
			}

			instanceObject = this;

			dialogSequenceSpawner = new CodeSpawner<ObjectChatSequence<SpeakerType, EmotionType, ActionType>>();
			chatBubbleSpawner = new UnitySpawner<BaseChat<SpeakerType, EmotionType, ActionType>>(chatBubblePrefab);
		}

		protected override BaseChatSequence<SpeakerType, EmotionType, ActionType> SpawnDialogSequence() {
			return dialogSequenceSpawner.Spawn();
		}

		protected override void OnSequenceFinished(SequenceBase sequence) {
			base.OnSequenceFinished(sequence);

			if (sequence is ObjectChatSequence<SpeakerType, EmotionType, ActionType> objectSequence) {
				objectSequence.Despawn();
			}
		}
	}
}