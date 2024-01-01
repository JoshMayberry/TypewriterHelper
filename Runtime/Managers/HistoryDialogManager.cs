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
	public class HistoryDialogManager<SpeakerType> : BaseDialogManager<SpeakerType> where SpeakerType : Enum {
		[Header("History: Setup")]
		[Required][SerializeField] public VerticalLayoutGroup chatBubbleContainer;
		[Readonly][SerializeField] public RectTransform chatBubbleContainerRectTransform;

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
			this.chatBubbleContainerRectTransform = this.chatBubbleContainer.GetComponent<RectTransform>();
        }

		protected override BaseChatSequence<SpeakerType> SpawnDialogSequence() {
			return dialogSequenceSpawner.Spawn();
		}

		public virtual void RefreshContainer() {
            LayoutRebuilder.ForceRebuildLayoutImmediate(this.chatBubbleContainerRectTransform);
        }
    }
}