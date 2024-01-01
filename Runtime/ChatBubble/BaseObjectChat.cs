using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using jmayberry.CustomAttributes;

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
		[Required][SerializeField] protected TextMeshPro dialogText;
		[Required][SerializeField] protected TextMeshPro speakerText;
		[Required][SerializeField] protected SpriteRenderer iconSpriteRenderer;
		[Required][SerializeField] protected SpriteRenderer backgroundSpriteRenderer;
		[Required][SerializeField] protected Transform container;

		[Header("Object: Tweak")]
		[Required][SerializeField] protected Vector2 padding = new Vector2(2f, 2f);
		[Required][SerializeField] protected Vector2 speakerNameOffset = new Vector2(-2f, 0f);
		[Required][SerializeField] protected Vector2 IconOffset = new Vector2(2f, 2f);
		[Required][SerializeField] protected Vector2 minBackgroundSize = new Vector2(1.2f, 1f);
		[Required][SerializeField] protected Vector2 constrainOnScreenPadding = new Vector2(2f, 2f);
		[Required][SerializeField] protected float minTextSize = 0f;
		[Required][SerializeField] protected float maxTextSize = 12f;
		[Required][SerializeField] protected bool growWithText = true;
		[Required][SerializeField] protected bool constrainOnScreen = true;

		public override void SoftReset(Transform newPosition = null) {
			this.dialogText.text = "";
			this.speakerText.text = "";
			this.container.transform.localPosition = Vector3.zero;
			this.iconSpriteRenderer.transform.localPosition = Vector3.zero;

			base.SoftReset(newPosition);

			this.DoHide();
		}

		public override void OnSpawn(object spawner) {
			this.SoftReset();
			this.chatBubbleInfo = DialogManagerBase<SpeakerType>.instance.fallbackChatBubbleInfo;
			DialogManagerBase<SpeakerType>.instance.EventUpdateBubblePosition.AddListener(this.UpdatePosition);
		}

		public override void OnDespawn(object spawner) {
			DialogManagerBase<SpeakerType>.instance.EventUpdateBubblePosition.RemoveListener(this.UpdatePosition);
		}

		public virtual void DoShow() {
			this.dialogText.gameObject.SetActive(true);
			this.speakerText.gameObject.SetActive(true);
			this.iconSpriteRenderer.gameObject.SetActive(true);
			this.backgroundSpriteRenderer.gameObject.SetActive(true);
		}

		public virtual void DoHide() {
			this.dialogText.gameObject.SetActive(false);
			this.speakerText.gameObject.SetActive(false);
			this.iconSpriteRenderer.gameObject.SetActive(false);
			this.backgroundSpriteRenderer.gameObject.SetActive(false);
		}

		protected override void UpdateSpeaker_setText(Speaker<SpeakerType> newSpeaker) {
			this.speakerText.text = this.UpdateSpeaker_getSpeakerName(newSpeaker);
			this.speakerText.ForceMeshUpdate(); // Ensure text renders this frame so we can get the size
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

		protected override IEnumerator Populate_PreLoop() {
			yield return this.Show();
		}

		protected override IEnumerator Populate_PrepareText(string dialog) {
			if (this.currentText != "") {
				float initialProgress = this.currentProgress;
				yield return this.doOverTime(this.initialAdjustSizeSpeed, (progress) => this.UpdateTextProgress(initialProgress * (1 - progress)));
			}

			this.currentText = dialog;
			if (this.growWithText) {
				this.dialogText.text = "";
				this.dialogText.maxVisibleCharacters = 99999;
			}
			else {
				this.dialogText.text = this.currentText;
				this.dialogText.maxVisibleCharacters = 0;
			}

			if (!this.growWithText) {
				this.UpdatePosition();
			}

			this.dialogText.rectTransform.sizeDelta = new Vector2(this.maxTextSize, 0); // Needed to calculate the background based on the text size
		}

		internal override void UpdateTextProgress(float progress) {
			base.UpdateTextProgress(progress);

			if (this.growWithText) {
				this.backgroundSpriteRenderer.size = this.UpdateTextProgress_getBackgroundSize();
				this.UpdatePosition();
			}
		}

		protected virtual Vector2 UpdateTextProgress_getBackgroundSize() {
			Vector2 textSize = this.dialogText.GetRenderedValues(false);
			Vector2 targetSize = (textSize + this.padding) / this.backgroundSpriteRenderer.transform.localScale;
			return new Vector2(Mathf.Max(this.minBackgroundSize.x, targetSize.x), Mathf.Max(this.minBackgroundSize.y, targetSize.y)); // Ensure the sprite does not get squished
		}

		protected override void UpdateTextProgress_SetText(int textLength) {
			if (this.growWithText) {
				this.dialogText.text = this.currentText[..textLength];
				this.dialogText.ForceMeshUpdate(); // Ensure text renders this frame so we can get the size
			}
			else {
				// See: https://github.com/aarthificial-unity/foundations/blob/7d43e288317085920a55ea61c09bf30f3371b33c/Assets/View/Dialogue/DialogueBubble.cs#L122
				this.dialogText.maxVisibleCharacters = textLength;
			}
		}

		protected virtual IEnumerator LerpOpacity(float initialOpacity, float targetOpacity, Action<float> extraLerp = null) {
			Color dialog_originalColor = new Color(this.dialogText.color.r, this.dialogText.color.g, this.dialogText.color.b, initialOpacity);
			Color dialog_targetColor = new Color(dialog_originalColor.r, dialog_originalColor.g, dialog_originalColor.b, targetOpacity);

			Color speaker_originalColor = new Color(this.speakerText.color.r, this.speakerText.color.g, this.speakerText.color.b, initialOpacity);
			Color speaker_targetColor = new Color(speaker_originalColor.r, speaker_originalColor.g, speaker_originalColor.b, targetOpacity);

			Color icon_originalColor = new Color(this.iconSpriteRenderer.color.r, this.iconSpriteRenderer.color.g, this.iconSpriteRenderer.color.b, initialOpacity);
			Color icon_targetColor = new Color(icon_originalColor.r, icon_originalColor.g, icon_originalColor.b, targetOpacity);

			Color background_originalColor = new Color(this.backgroundSpriteRenderer.color.r, this.backgroundSpriteRenderer.color.g, this.backgroundSpriteRenderer.color.b, initialOpacity);
			Color background_targetColor = new Color(background_originalColor.r, background_originalColor.g, background_originalColor.b, targetOpacity);

			yield return this.doOverTime(this.initialAdjustSizeSpeed, (float progress) => {
				this.dialogText.color = Color.Lerp(dialog_originalColor, dialog_targetColor, progress);
				this.speakerText.color = Color.Lerp(speaker_originalColor, speaker_targetColor, progress);
				this.iconSpriteRenderer.color = Color.Lerp(icon_originalColor, icon_targetColor, progress);
				this.backgroundSpriteRenderer.color = Color.Lerp(background_originalColor, background_targetColor, progress);
				extraLerp?.Invoke(progress);
			});
		}

		public virtual IEnumerator Show() {
			if (this.dialogText.gameObject.activeSelf) {
				yield break;
			}

			this.dialogText.text = "";
			this.DoShow();
			this.backgroundSpriteRenderer.size = this.minBackgroundSize;
			this.dialogText.ForceMeshUpdate(); // Ensure text renders this frame so we can get the size
			yield return this.LerpOpacity(0, 1);
		}

		public virtual IEnumerator Hide() {
			if (!this.dialogText.gameObject.activeSelf) {
				yield break;
			}

			if (this.currentProgress >= 0) {
				float initialProgress = this.currentProgress;
				yield return this.LerpOpacity(1, 0, (progress) => this.UpdateTextProgress(initialProgress * (1 - progress)));
			}
			else {
				yield return this.LerpOpacity(1, 0);
			}
			this.DoHide();
		}

		protected internal override IEnumerator OnFinishedSequence() {
			yield return this.Hide();
		}

		protected internal override IEnumerator DespawnCoroutine() {
			yield return this.Hide();
			DialogManagerBase<SpeakerType>.chatBubbleSpawner.Despawn(this);
		}
	}
}
