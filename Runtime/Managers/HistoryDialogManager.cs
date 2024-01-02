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
    public class HistoryChatBubbleInfo : BaseChatBubbleInfo { }

    public class HistoryDialogManager<SpeakerType> : BaseDialogManager<SpeakerType> where SpeakerType : Enum {
		[Header("History: Setup")]
		[Required][SerializeField] protected BaseChat<SpeakerType> chatBubblePrefab;
        [Required][SerializeField] public RectTransform chatBubbleContainerToHide;
		[Required][SerializeField] public VerticalLayoutGroup chatBubbleContainer;
        [Readonly][SerializeField] public RectTransform chatBubbleContainerRectTransform;

        [SerializedDictionary("Speaker Type", "Chat Bubble")] public SerializedDictionary<SpeakerType, HistoryChatBubbleInfo> chatBubbleInfo = new SerializedDictionary<SpeakerType, HistoryChatBubbleInfo>();
        public HistoryChatBubbleInfo fallbackChatBubbleInfo;

        protected internal static CodeSpawner<HistoryChatSequence<SpeakerType>> dialogSequenceSpawner;

		public static HistoryDialogManager<SpeakerType> instanceHistory { get; private set; }

		protected override void Awake() {
			base.Awake();

			if (instanceHistory != null && instanceHistory != this) {
				Debug.LogError("Found more than one HistoryDialogManager<SpeakerType> in the scene.");
				Destroy(this.gameObject);
				return;
			}

			instanceHistory = this;

			dialogSequenceSpawner = new CodeSpawner<HistoryChatSequence<SpeakerType>>();
			chatBubbleSpawner = new UnitySpawner<BaseChat<SpeakerType>>(chatBubblePrefab);

			this.chatBubbleContainerRectTransform = this.chatBubbleContainer.GetComponent<RectTransform>();

            this.HideChat();
        }

		protected override BaseChatSequence<SpeakerType> SpawnDialogSequence() {
			return dialogSequenceSpawner.Spawn();
		}

		public virtual void RefreshContainer() {
            LayoutRebuilder.ForceRebuildLayoutImmediate(this.chatBubbleContainerRectTransform);
        }

		protected internal virtual void Clear() {
			List<HistoryBubbleChat<SpeakerType>> removeList = new List<HistoryBubbleChat<SpeakerType>>();
            foreach (RectTransform childTransform in this.chatBubbleContainerRectTransform) {
				HistoryBubbleChat<SpeakerType> child = childTransform.GetComponent<HistoryBubbleChat<SpeakerType>>();
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

            if (sequence is HistoryChatSequence<SpeakerType> historySequence) {
                historySequence.Despawn();
            }
        }
    }
}