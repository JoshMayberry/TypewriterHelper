using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

using jmayberry.CustomAttributes;
using jmayberry.Spawner;
using jmayberry.TypewriterHelper.Entries;
using Aarthificial.Typewriter;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using Unity.Plastic.Newtonsoft.Json.Linq;

namespace jmayberry.TypewriterHelper {
	/**
	 * This kind of chat bubble shows up on the UI inside a layout container.
	 * When the chat is over, the bubble becomes semi-transparent and does not despawn until the entire sequence is over. 
	 */
	[Serializable]
	public abstract class HistoryBubbleChat<SpeakerType, EmotionType, ActionType> : BaseChat<SpeakerType, EmotionType, ActionType> where SpeakerType : Enum where EmotionType : Enum where ActionType : Enum {
		[Header("History: Setup")]
		[Required][SerializeField] protected Image iconImage;
		[Required][SerializeField] protected Image backgroundImage;
		[Required][SerializeField] protected RectTransform container;

		[Header("History: Tweak")]
		[SerializeField] protected Vector3 initialContainerScale = new Vector3(1, 1, 1);
		[SerializeField] protected Vector2 containerPadding = new Vector2(10, 0); // For scrollbars

		[Header("History: Debug")]
		[Readonly] [SerializeField] protected List<BaseChat<SpeakerType, EmotionType, ActionType>> subBubbleList;
		[SerializeField] protected BaseChat<SpeakerType, EmotionType, ActionType> subBubbleLatest;
		[Readonly][SerializeField] protected internal HistoryChatBubbleInfo<EmotionType> chatBubbleInfo;

		protected void Awake() {
			this.subBubbleList = new List<BaseChat<SpeakerType, EmotionType, ActionType>>();
		}

		protected virtual void Clear() {
			foreach (var subBubble in this.subBubbleList) {
				subBubble.Despawn();
			}

			this.subBubbleList.Clear();
		}

		public override void SoftReset(Transform newPosition = null) {
			this.Clear();
			base.SoftReset(newPosition);
		}

		public override void OnSpawn(object spawner) {
			base.OnSpawn(spawner);
			this.transform.SetParent(HistoryDialogManager<SpeakerType, EmotionType, ActionType>.instanceHistory.chatBubbleContainer.transform);
		}

		public override void OnDespawn(object spawner) {
			base.OnDespawn(spawner);
			this.transform.SetParent(HistoryDialogManager<SpeakerType, EmotionType, ActionType>.instanceHistory.transform);
			this.Clear();
		}

		protected override void SetChatBubbleInfo(Speaker<SpeakerType, EmotionType, ActionType> speaker) {
			var fallbackChatBubbleInfo = HistoryDialogManager<SpeakerType, EmotionType, ActionType>.instanceHistory.fallbackChatBubbleInfo;

			if (speaker == null) {
				this.chatBubbleInfo = fallbackChatBubbleInfo;
			}
			else {
				this.chatBubbleInfo = HistoryDialogManager<SpeakerType, EmotionType, ActionType>.instanceHistory.chatBubbleInfo.GetValueOrDefault(speaker.speakerType, fallbackChatBubbleInfo);
			}
		}

		protected override BaseSpeakerVoice UpdateSpeaker_getSpeakerVoice(Speaker<SpeakerType, EmotionType, ActionType> newSpeaker) {
			return this.chatBubbleInfo.speakerVoice.GetValueOrDefault(this.currentEmotion, (newSpeaker.speakerVoice != null ? newSpeaker.speakerVoice : this.chatBubbleInfo.fallbackSpeakerVoice));
		}

		protected override void UpdateContainerSize(Vector2 newSize) {
			this.container.sizeDelta = newSize;
			HistoryDialogManager<SpeakerType, EmotionType, ActionType>.instanceHistory.RefreshContainer();
		}

		protected virtual Vector2 GetContainerScale() {
			return this.container.localScale;
		}

		protected override Vector2 GetContainerTargetSize() {
			Vector2 textSize = this.dialogText.GetRenderedValues(false);
			Vector2 targetSize = (textSize + this.padding);
			float availableWidth = HistoryDialogManager<SpeakerType, EmotionType, ActionType>.instanceHistory.chatBubbleContainerRectTransform.rect.width - this.containerPadding.x;

			return new Vector2(availableWidth, Mathf.Max(this.minContainerSize.y, targetSize.y)) / this.GetContainerScale();
		}

		protected override bool UpdateSprites() {
			if (this.chatBubbleInfo == null) {
				Debug.Log("Cannot update icon");
				return false;
			}

			this.iconImage.sprite = this.chatBubbleInfo.iconSprite.GetValueOrDefault(this.currentDialogOption, this.chatBubbleInfo.fallbackIconSprite);
			this.backgroundImage.sprite = this.chatBubbleInfo.backgroundSprite.GetValueOrDefault(this.currentChatBubbleType, this.chatBubbleInfo.fallbackBackground);
			return true;
		}

		protected override void SetSpriteActive(bool state) {
			this.iconImage.gameObject.SetActive(state);
			this.backgroundImage.gameObject.SetActive(state);
		}

		protected override Color GetSpriteColor() {
			return this.backgroundImage.color;
		}

		protected override void SetSpriteColor(Color newColor) {
			this.iconImage.color = newColor;
			this.backgroundImage.color = newColor;
		}

		protected internal override IEnumerator Populate_PreLoop(DialogContext dialogContext, BaseDialogEntry<EmotionType> BaseDialogEntry) {
			this.container.localScale = initialContainerScale; // Patch for scale changing for some reason
			yield return base.Populate_PreLoop(dialogContext, BaseDialogEntry);
		}

		public override IEnumerator Populate_Loop(DialogContext dialogContext, BaseDialogEntry<EmotionType> BaseDialogEntry, string dialog, int i, bool isLastInLoop, bool isLastInSequence) {
			if (i == 0) {
				yield return base.Populate_Loop(dialogContext, BaseDialogEntry, dialog, i, isLastInLoop, isLastInSequence);
				yield break;
			}


			this.SetOpacity(0.6f);

			this.subBubbleLatest = this.spawner.Spawn();
			this.subBubbleList.Add(this.subBubbleLatest);

			yield return this.subBubbleLatest.Populate_PreLoop(dialogContext, BaseDialogEntry);
			yield return this.subBubbleLatest.Populate_Loop(dialogContext, BaseDialogEntry, dialog, 0, isLastInLoop, isLastInSequence);
		}

		public override void SetOpacity(float value) {
			base.SetOpacity(value);

			if (this.subBubbleLatest != null) {
				this.subBubbleLatest.SetOpacity(value);
			}
		}

		public override void OnSkipToEnd() {
			base.OnSkipToEnd();

			if (this.subBubbleLatest != null) {
				this.subBubbleLatest.OnSkipToEnd();
			}
		}
	}
}