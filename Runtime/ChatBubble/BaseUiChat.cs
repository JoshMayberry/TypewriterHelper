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
using Unity.VisualScripting.YamlDotNet.Core;

namespace jmayberry.TypewriterHelper {
	[Serializable]
	public abstract class BaseUiChat<SpeakerType> : BaseChat<SpeakerType> where SpeakerType : Enum {
		[Header("Ui: Setup")]
		[Required][SerializeField] protected Image iconImage;
		[Required][SerializeField] protected Image backgroundImage;
		[Required][SerializeField] protected RectTransform container;

		[Header("Ui: Tweak")]
        [SerializeField] protected Vector3 initialContainerScale = new Vector3(1, 1, 1);
        [SerializeField] protected Vector2 containerPadding = new Vector2(-20, 0); // For scrollbars

        protected override void SetSpriteActive(bool state) {
			this.iconImage.gameObject.SetActive(state);
			this.backgroundImage.gameObject.SetActive(state);
		}

		protected override void UpdateContainerSize(Vector2 newSize) {
			this.container.sizeDelta = newSize;
            HistoryDialogManager<SpeakerType>.instanceHistory.RefreshContainer();
        }

		protected override Vector2 GetContainerScale() {
			return this.container.localScale;
        }

        protected override Vector2 GetContainerTargetSize() {
            Vector2 textSize = this.dialogText.GetRenderedValues(false);
            Vector2 targetSize = (textSize + this.padding);
			float availableWidth = HistoryDialogManager<SpeakerType>.instanceHistory.chatBubbleContainerRectTransform.rect.width - this.containerPadding.x;

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
		protected override Color GetSpriteColor() {
			return this.backgroundImage.color;
		}

		protected override void SetSpriteColor(Color newColor) {
			this.iconImage.color = newColor;
			this.backgroundImage.color = newColor;
		}

		protected override IEnumerator Populate_PreLoop() {
			this.container.localScale = initialContainerScale; // Patch for scale changing for some reason
			yield return null;
        }

		//protected override IEnumerator Populate_PrepareText(string dialog) {
		//	yield return base.Populate_PrepareText(dialog);

		//	this.dialogText.rectTransform.sizeDelta = new Vector2(this.maxTextSize, 0); // Needed to calculate the background based on the text size
		//}
	}
}
