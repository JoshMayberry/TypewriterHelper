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

	public class HistoryDialogManager<SpeakerType, EmotionType, ActionType> : BaseDialogManager<SpeakerType, EmotionType, ActionType> where SpeakerType : Enum where EmotionType : Enum where ActionType : Enum {
		[Header("History: Setup")]
		[Required][SerializeField] protected BaseChat<SpeakerType, EmotionType, ActionType> chatBubblePrefab;
		[Required][SerializeField] public RectTransform chatBubbleContainerToHide;
		[Required][SerializeField] public VerticalLayoutGroup chatBubbleContainer;
		[Readonly][SerializeField] public RectTransform chatBubbleContainerRectTransform;

		[SerializedDictionary("Speaker Type", "Chat Bubble")] public SerializedDictionary<SpeakerType, HistoryChatBubbleInfo<EmotionType>> chatBubbleInfo = new SerializedDictionary<SpeakerType, HistoryChatBubbleInfo<EmotionType>>();
		public HistoryChatBubbleInfo<EmotionType> fallbackChatBubbleInfo;

		protected internal static CodeSpawner<HistoryChatSequence<SpeakerType, EmotionType, ActionType>> dialogSequenceSpawner;

		public static HistoryDialogManager<SpeakerType, EmotionType, ActionType> instanceHistory { get; private set; }

		protected override void Awake() {
			base.Awake();

			if (instanceHistory != null && instanceHistory != this) {
				Debug.LogError("Found more than one HistoryDialogManager<SpeakerType, EmotionType, ActionType> in the scene.");
				Destroy(this.gameObject);
				return;
			}

			instanceHistory = this;

			dialogSequenceSpawner = new CodeSpawner<HistoryChatSequence<SpeakerType, EmotionType, ActionType>>();
			chatBubbleSpawner = new UnitySpawner<BaseChat<SpeakerType, EmotionType, ActionType>>(chatBubblePrefab);

			this.chatBubbleContainerRectTransform = this.chatBubbleContainer.GetComponent<RectTransform>();

			this.HideChat();
		}

		protected override BaseChatSequence<SpeakerType, EmotionType, ActionType> SpawnDialogSequence() {
			return dialogSequenceSpawner.Spawn();
		}

		public virtual void RefreshContainer() {
			LayoutRebuilder.ForceRebuildLayoutImmediate(this.chatBubbleContainerRectTransform);
		}

		protected internal virtual void Clear() {
			List<HistoryBubbleChat<SpeakerType, EmotionType, ActionType>> removeList = new List<HistoryBubbleChat<SpeakerType, EmotionType, ActionType>>();
			foreach (RectTransform childTransform in this.chatBubbleContainerRectTransform) {
				HistoryBubbleChat<SpeakerType, EmotionType, ActionType> child = childTransform.GetComponent<HistoryBubbleChat<SpeakerType, EmotionType, ActionType>>();
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

			if (sequence is HistoryChatSequence<SpeakerType, EmotionType, ActionType> historySequence) {
				historySequence.Despawn();
			}
		}
	}
}