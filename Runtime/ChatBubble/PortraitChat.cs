using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

using jmayberry.CustomAttributes;
using jmayberry.Spawner;
using jmayberry.TypewriterHelper.Entries;
using Aarthificial.Typewriter;

namespace jmayberry.TypewriterHelper {
	public enum PortraitSide {
		Right,
		Left,
	}

	/**
	 * This kind of chat bubble exists on the UI.
	 * It is not spawned in or despawned.
	 */
	[Serializable]
	public abstract class PortraitChat<SpeakerType, EmotionType> : BaseChat<SpeakerType> where SpeakerType : Enum where EmotionType : Enum {
		[Header("Portrait: Setup")]
		[Required][SerializeField] protected Image iconImage;
		[Required][SerializeField] protected Image backgroundImage;
		[Required][SerializeField] protected Image portraitImageLeft;
		[Required][SerializeField] protected Image portraitImageRight;
        [Required][SerializeField] protected RectTransform container;

        [Header("Portrait: Tweak")]
		[SerializeField] protected Color activePortraitColor = new Color(1, 1, 1, 1);
		[SerializeField] protected Color inactivePortraitColor = new Color(1, 1, 1, 0.6f);

		[Header("Portrait: Debug")]
		[Readonly][SerializeField] protected internal PortraitChatBubbleInfo<EmotionType> chatBubbleInfo;
		[Readonly][SerializeField] protected internal BaseEmoteDialogEntry<EmotionType> currentEmoteEntry;
		[Readonly] public BaseEmoteDialogEntry<EmotionType> currentEmoteEntry2;
        [Readonly][SerializeField] protected internal PortraitSide currentSide;
		[Readonly][SerializeField] protected internal EmotionType currentEmotion;

        public override void SoftReset(Transform newPosition = null) {
			this.currentEmoteEntry = null;
			this.currentSide = PortraitSide.Right;
			this.portraitImageLeft.gameObject.SetActive(false);
			this.portraitImageRight.gameObject.SetActive(false);
            base.SoftReset(newPosition);
        }
        protected internal override IEnumerator Populate_PreLoop(DialogContext dialogContext, DialogEntry dialogEntry) {
            if (dialogEntry is BaseEmoteDialogEntry<EmotionType> emoteDialogEntry) {
                this.currentEmoteEntry = emoteDialogEntry;
				this.currentEmotion = emoteDialogEntry.emotion;
            }
			else {
                this.currentEmoteEntry = null;
            }

            this.Populate_UpdateSide();
            yield return base.Populate_PreLoop(dialogContext, dialogEntry);
        }

        protected virtual void Populate_UpdateSide() {
            var newSide = (this.currentEmoteEntry == null) ? PortraitSide.Right : this.currentEmoteEntry.portraitSide;
            this.Populate_SetSideActive(newSide);
        }

        protected virtual void Populate_SetSideActive(PortraitSide newSide) {
            if (newSide == PortraitSide.Left) {
                this.portraitImageRight.color = this.inactivePortraitColor;
                this.portraitImageLeft.color = this.activePortraitColor;
                this.portraitImageLeft.gameObject.SetActive(true);
            }
            else {
                this.portraitImageLeft.color = this.inactivePortraitColor;
                this.portraitImageRight.color = this.activePortraitColor;
                this.portraitImageRight.gameObject.SetActive(true);
            }

            this.currentSide = newSide;
        }

        protected override bool UpdateSpeaker_noNewSpeaker() {
			return this.UpdateSprites_Portrait(); // Account for same speaker but different emotion
		}

		protected override void SetChatBubbleInfo(Speaker<SpeakerType> speaker) {
			var fallbackChatBubbleInfo = PortraitDialogManager<SpeakerType, EmotionType>.instancePortrait.fallbackChatBubbleInfo;

			if (speaker == null) {
				this.chatBubbleInfo = fallbackChatBubbleInfo;
			}
			else {
				this.chatBubbleInfo = PortraitDialogManager<SpeakerType, EmotionType>.instancePortrait.chatBubbleInfo.GetValueOrDefault(speaker.speakerType, fallbackChatBubbleInfo);
			}
		}

		protected override bool UpdateSprites() {
			if (this.chatBubbleInfo == null) {
				Debug.Log("Cannot update icon");
				return false;
			}

			this.iconImage.sprite = this.chatBubbleInfo.iconSprite.GetValueOrDefault(this.currentDialogOption, this.chatBubbleInfo.fallbackIconSprite);
			this.backgroundImage.sprite = this.chatBubbleInfo.backgroundSprite.GetValueOrDefault(this.currentChatBubbleType, this.chatBubbleInfo.fallbackBackground);

			return UpdateSprites_Portrait();
		}

		protected virtual bool UpdateSprites_Portrait() {
			//Debug.Log($"@UpdateSprites_Portrait {this.currentEmotion}; {this.currentSide}"); // TODO: Called too often?
            Sprite newSprite = ((this.currentEmoteEntry == null) ? this.chatBubbleInfo.fallbackPortraitSprite : this.chatBubbleInfo.portraitSprite.GetValueOrDefault(this.currentEmotion, this.chatBubbleInfo.fallbackPortraitSprite));
            if (this.currentSide == PortraitSide.Left) {
				this.portraitImageLeft.sprite = newSprite;
			}
			else {
                this.portraitImageRight.sprite = newSprite;
            }

            return true;
		}

		protected override void SetSpriteActive(bool state) {
            this.iconImage.gameObject.SetActive(state);
            this.backgroundImage.gameObject.SetActive(state);

			if (!state) {
                this.portraitImageLeft.gameObject.SetActive(false);
                this.portraitImageRight.gameObject.SetActive(false);
            }
        }

        protected override void UpdateContainerSize(Vector2 newSize) {
            this.container.sizeDelta = newSize;
        }

        protected virtual Vector2 GetContainerScale() {
            return this.container.localScale;
        }

        protected override Vector2 GetContainerTargetSize() {
            Vector2 textSize = this.dialogText.GetRenderedValues(false);
            Vector2 targetSize = (textSize + this.padding);

            return new Vector2(default, Mathf.Max(this.minContainerSize.y, targetSize.y)) / this.GetContainerScale();
        }

        protected override Color GetSpriteColor() {
			return Color.white;
        }

        protected override void SetSpriteColor(Color newColor) { }
    }
}