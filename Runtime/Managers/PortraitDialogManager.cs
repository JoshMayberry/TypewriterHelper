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
using UnityEngine.UI;

namespace jmayberry.TypewriterHelper {
	[Serializable]
	public class PortraitChatBubbleInfo<EmotionType> : BaseChatBubbleInfo<EmotionType> where EmotionType : Enum {
		[Required] public Sprite fallbackPortraitSprite;
		[SerializedDictionary("Emotion Type", "Portrait")] public SerializedDictionary<EmotionType, Sprite> portraitSprite;
	}

	public class PortraitDialogManager<SpeakerType, EmotionType, ActionType> : BaseDialogManager<SpeakerType, EmotionType, ActionType> where SpeakerType : Enum where EmotionType : Enum where ActionType : Enum {
		[Header("Portrait: Setup")]
		[Required][SerializeField] public PortraitChat<SpeakerType, EmotionType, ActionType> chatWindow;
		[SerializedDictionary("Speaker Type", "Chat Bubble")] public SerializedDictionary<SpeakerType, PortraitChatBubbleInfo<EmotionType>> chatBubbleInfo = new SerializedDictionary<SpeakerType, PortraitChatBubbleInfo<EmotionType>>();
		public PortraitChatBubbleInfo<EmotionType> fallbackChatBubbleInfo;

		protected internal static CodeSpawner<PortraitChatSequence<SpeakerType, EmotionType, ActionType>> dialogSequenceSpawner;

		public static PortraitDialogManager<SpeakerType, EmotionType, ActionType> instancePortrait { get; private set; }

		protected override void Awake() {
			base.Awake();

			if (instancePortrait != null && instancePortrait != this) {
				Debug.LogError("Found more than one PortraitDialogManager<SpeakerType, EmotionType, ActionType> in the scene.");
				Destroy(this.gameObject);
				return;
			}

			instancePortrait = this;

			dialogSequenceSpawner = new CodeSpawner<PortraitChatSequence<SpeakerType, EmotionType, ActionType>>();

			this.chatWindow.DoHide();
		}

		protected override BaseChatSequence<SpeakerType, EmotionType, ActionType> SpawnDialogSequence() {
			var sequence = dialogSequenceSpawner.Spawn();
			sequence.chatBubble = this.chatWindow;
			return sequence;
		}

		protected internal virtual void ShowChat() {
			StartCoroutine(this.chatWindow.Show());
		}

		protected internal virtual void HideChat() {
			StartCoroutine(this.chatWindow.Hide());
		}

		protected override void OnSequenceFinished(SequenceBase sequence) {
			base.OnSequenceFinished(sequence);

			if (sequence is PortraitChatSequence<SpeakerType, EmotionType, ActionType> portraitSequence) {
				portraitSequence.Despawn();
			}
		}
	}
}