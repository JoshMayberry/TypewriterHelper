using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using jmayberry.CustomAttributes;
using jmayberry.Spawner;

namespace jmayberry.TypewriterHelper {
	public enum ChatBubbleAlignment {
		TopLeft,
		TopMiddle,
		TopRight,
		BottomLeft,
		BottomMiddle,
		BottomRight,
		Left,
		Right,
		Center,
	}

	[Serializable]
	public abstract class BaseObjectChat<SpeakerType> : BaseChat<SpeakerType> where SpeakerType : Enum {
		[Header("Object: Setup")]
		[Required][SerializeField] protected SpriteRenderer iconSpriteRenderer;
		[Required][SerializeField] protected SpriteRenderer backgroundSpriteRenderer;
		[Required][SerializeField] protected Transform container;

		[Header("Object: Tweak")]
		[Required][SerializeField] protected Vector2 speakerNameOffset = new Vector2(-2f, 0f);
        [Required][SerializeField] protected Vector2 IconOffset = new Vector2(2f, 2f);
		[Required][SerializeField] protected Vector2 constrainOnScreenPadding = new Vector2(2f, 2f);
		//[Required][SerializeField] protected float minTextSize = 0f;
		[Required][SerializeField] protected float maxTextSize = 12f;
		[Required][SerializeField] protected bool constrainOnScreen = true;

		public override void SoftReset(Transform newPosition = null) {
			this.backgroundSpriteRenderer.transform.localPosition = Vector3.zero;
			this.iconSpriteRenderer.transform.localPosition = Vector3.zero;
			base.SoftReset(newPosition);
		}

		public override void OnSpawn(object spawner) {
			base.OnSpawn(spawner);
			BaseDialogManager<SpeakerType>.instance.EventUpdateBubblePosition.AddListener(this.UpdatePosition);
		}

		public override void OnDespawn(object spawner) {
			base.OnDespawn(spawner);
			BaseDialogManager<SpeakerType>.instance.EventUpdateBubblePosition.RemoveListener(this.UpdatePosition);
		}

		protected override void SetSpriteActive(bool state) {
			this.iconSpriteRenderer.gameObject.SetActive(state);
			this.backgroundSpriteRenderer.gameObject.SetActive(state);
		}

		protected override void UpdateContainerSize(Vector2 newSize) {
			this.backgroundSpriteRenderer.size = newSize;
		}

		protected override Vector2 GetContainerScale() {
			return this.backgroundSpriteRenderer.transform.localScale;
		}

		protected override bool UpdateSprites() {
			if (this.chatBubbleInfo == null) {
				Debug.Log("Cannot update icon");
				return false;
			}

			this.iconSpriteRenderer.sprite = this.chatBubbleInfo.iconSprite.GetValueOrDefault(this.currentDialogOption, this.chatBubbleInfo.fallbackIconSprite);
			this.backgroundSpriteRenderer.sprite = this.chatBubbleInfo.backgroundSprite.GetValueOrDefault(this.currentChatBubbleType, this.chatBubbleInfo.fallbackBackground);
			return true;
		}
		protected override Color GetSpriteColor() {
			return this.backgroundSpriteRenderer.color;
		}

		protected override void SetSpriteColor(Color newColor) {
			this.iconSpriteRenderer.color = newColor;
			this.backgroundSpriteRenderer.color = newColor;
		}

		protected override IEnumerator Populate_PrepareText(string dialog) {
			yield return base.Populate_PrepareText(dialog);

			if (!this.growWithText) {
				this.UpdatePosition();
			}

			this.dialogText.rectTransform.sizeDelta = new Vector2(this.maxTextSize, 0); // Needed to calculate the background based on the text size
		}

		internal override void UpdateTextProgress(float progress) {
			base.UpdateTextProgress(progress);

			if (this.growWithText) {
				this.UpdatePosition();
			}
		}

        protected override Vector2 GetContainerTargetSize() {
            Vector2 textSize = this.dialogText.GetRenderedValues(false);
            Vector2 targetSize = (textSize + this.padding);
            return new Vector2(Mathf.Max(this.minContainerSize.x, targetSize.x), Mathf.Max(this.minContainerSize.y, targetSize.y)) / this.GetContainerScale();
        }

        public virtual void UpdatePosition() {
			if (this.currentSpeaker == null) {
				return;
			}

			Vector2 backgroundSize = this.backgroundSpriteRenderer.size * this.backgroundSpriteRenderer.transform.localScale;
			this.container.transform.position = this.UpdatePosition_getBackgroundPosition(this.currentSpeaker.chatBubblePosition.position, backgroundSize, this.currentSpeaker.chatBubbleAlignment);
			this.iconSpriteRenderer.transform.position = this.UpdatePosition_getIconPosition(this.container.transform.position, backgroundSize, this.iconSpriteRenderer.size, this.IconOffset);
			this.speakerText.gameObject.transform.position = this.UpdatePosition_getSpeakerPosition(this.container.transform.position, backgroundSize, this.speakerNameOffset);
		}

		protected virtual Vector2 UpdatePosition_getBackgroundPosition(Vector2 position, Vector2 backgroundSize, ChatBubbleAlignment alignment) {
			switch (alignment) {
				case ChatBubbleAlignment.TopLeft:
					position += new Vector2(-backgroundSize.x / 2, backgroundSize.y / 2);
					break;

				case ChatBubbleAlignment.TopMiddle:
					position += new Vector2(0, backgroundSize.y / 2);
					break;

				case ChatBubbleAlignment.TopRight:
					position += new Vector2(backgroundSize.x / 2, backgroundSize.y / 2);
					break;

				case ChatBubbleAlignment.BottomLeft:
					position += new Vector2(-backgroundSize.x / 2, -backgroundSize.y / 2);
					break;

				case ChatBubbleAlignment.BottomMiddle:
					position += new Vector2(0, -backgroundSize.y / 2);
					break;

				case ChatBubbleAlignment.BottomRight:
					position += new Vector2(backgroundSize.x / 2, -backgroundSize.y / 2);
					break;

				case ChatBubbleAlignment.Left:
					position += new Vector2(-backgroundSize.x / 2, 0);
					break;

				case ChatBubbleAlignment.Right:
					position += new Vector2(backgroundSize.x / 2, 0);
					break;

				case ChatBubbleAlignment.Center:
					// no need to modify the position
					break;

				default:
					Debug.Log($"Unknown alignment {alignment}");
					break;
			}

			if (this.constrainOnScreen) {
				Vector2 screenBottomLeft = Camera.main.ViewportToWorldPoint(new Vector2(0, 0));
				Vector2 screenTopRight = Camera.main.ViewportToWorldPoint(new Vector2(1, 1));

				float halfWidth = backgroundSize.x / 2;
				float halfHeight = backgroundSize.y / 2;

				position.x = Mathf.Clamp(position.x, screenBottomLeft.x + halfWidth + constrainOnScreenPadding.x, screenTopRight.x - halfWidth - constrainOnScreenPadding.x);
				position.y = Mathf.Clamp(position.y, screenBottomLeft.y + halfHeight + constrainOnScreenPadding.y, screenTopRight.y - halfHeight - constrainOnScreenPadding.y);
			}

			return position;
		}

		protected virtual Vector2 UpdatePosition_getIconPosition(Vector2 basePosition, Vector2 backgroundSize, Vector2 iconSize, Vector2 offsetPosition) {
			// Move icon to bottom right corner
			return basePosition + new Vector2(backgroundSize.x / 2 - iconSize.x / 2 + offsetPosition.x, -backgroundSize.y / 2 + iconSize.y / 2 + offsetPosition.y);
		}

		protected virtual Vector2 UpdatePosition_getSpeakerPosition(Vector2 basePosition, Vector2 backgroundSize, Vector2 offsetPosition) {
			// Move name to top left corner
			return basePosition + new Vector2(-backgroundSize.x / 2 + 1 + offsetPosition.x, backgroundSize.y / 2 + offsetPosition.y);
		}
	}
}
