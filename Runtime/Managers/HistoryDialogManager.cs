using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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

namespace jmayberry.TypewriterHelper {
	[Serializable]
	public class HistoryChatBubbleInfo<EmotionType> : BaseChatBubbleInfo<EmotionType> where EmotionType : Enum { }

	public class HistoryDialogManager<SpeakerType, EmotionType> : BaseDialogManager<SpeakerType, EmotionType> where SpeakerType : Enum where EmotionType : Enum {
		[Header("History: Setup")]
		[Required][SerializeField] protected BaseChat<SpeakerType, EmotionType> chatBubblePrefab;
		[Required][SerializeField] public RectTransform chatBubbleContainerToHide;
		[Required][SerializeField] public VerticalLayoutGroup chatBubbleContainer;
		[Readonly][SerializeField] public RectTransform chatBubbleContainerRectTransform;

		[SerializedDictionary("Speaker Type", "Chat Bubble")] public SerializedDictionary<SpeakerType, HistoryChatBubbleInfo<EmotionType>> chatBubbleInfo = new SerializedDictionary<SpeakerType, HistoryChatBubbleInfo<EmotionType>>();
		public HistoryChatBubbleInfo<EmotionType> fallbackChatBubbleInfo;

		protected internal static CodeSpawner<HistoryChatSequence<SpeakerType, EmotionType>> dialogSequenceSpawner;

		public static HistoryDialogManager<SpeakerType, EmotionType> instanceHistory { get; private set; }

		protected override void Awake() {
			base.Awake();

			if (instanceHistory != null && instanceHistory != this) {
				Debug.LogError("Found more than one HistoryDialogManager<SpeakerType, EmotionType> in the scene.");
				Destroy(this.gameObject);
				return;
			}

			instanceHistory = this;

			dialogSequenceSpawner = new CodeSpawner<HistoryChatSequence<SpeakerType, EmotionType>>();
			chatBubbleSpawner = new UnitySpawner<BaseChat<SpeakerType, EmotionType>>(chatBubblePrefab);

			this.chatBubbleContainerRectTransform = this.chatBubbleContainer.GetComponent<RectTransform>();

			this.HideChat();
		}

		protected override BaseChatSequence<SpeakerType, EmotionType> SpawnDialogSequence() {
			return dialogSequenceSpawner.Spawn();
		}

		public virtual void RefreshContainer() {
			LayoutRebuilder.ForceRebuildLayoutImmediate(this.chatBubbleContainerRectTransform);
		}

		protected internal virtual void Clear() {
			List<HistoryBubbleChat<SpeakerType, EmotionType>> removeList = new List<HistoryBubbleChat<SpeakerType, EmotionType>>();
			foreach (RectTransform childTransform in this.chatBubbleContainerRectTransform) {
				HistoryBubbleChat<SpeakerType, EmotionType> child = childTransform.GetComponent<HistoryBubbleChat<SpeakerType, EmotionType>>();
				if (child != null) {
					removeList.Add(child);
				}
			}

			foreach (var child in removeList) {
				chatBubbleSpawner.ShouldBeInactive(child);
			}
		}

		protected internal virtual void ShowChat() {
			this.Clear();
			this.chatBubbleContainerToHide.gameObject.SetActive(true);
		}

		protected internal virtual void HideChat() {
			this.chatBubbleContainerToHide.gameObject.SetActive(false);
			this.Clear();
		}

		public override void StopSequence(SequenceBase sequence, Coroutine coroutine, bool hardStop = false) {
			base.StopSequence(sequence, coroutine, hardStop);

			if (!this.isOverriding) {
				this.HideChat();
			}
		}

		protected override void OnSequenceFinished(SequenceBase sequence) {
			base.OnSequenceFinished(sequence);

			if (sequence is HistoryChatSequence<SpeakerType, EmotionType> historySequence) {
				historySequence.Despawn();
			}
		}
	}
}