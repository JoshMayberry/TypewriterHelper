using UnityEngine;
using UnityEngine.Events;
using TMPro;

using jmayberry.CustomAttributes;
using jmayberry.Spawner;
using System.Collections;
using jmayberry.TypewriterHelper.Entries;
using Aarthificial.Typewriter;
using System;
using System.Collections.Generic;

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
		Center
	}

	[Serializable]
	public abstract class ChatBubbleBase<SpeakerType> : MonoBehaviour, ISpawnable where SpeakerType : Enum {
		[Header("Setup")]
		[Required][SerializeField] protected TextMeshPro dialogText;
		[Required][SerializeField] protected TextMeshPro speakerText;
		[Required][SerializeField] protected SpriteRenderer iconSpriteRenderer;
		[Required][SerializeField] protected SpriteRenderer backgroundSpriteRenderer;
		[Required][SerializeField] protected Transform container;

		[Header("Tweak")]
		[Required][SerializeField] protected float charsPerSecond = 32f;
		[Required][SerializeField] protected float initialAdjustSizeSpeed = 0.1f;
		[Required][SerializeField] protected Vector2 padding = new Vector2(2f, 2f);
		[Required][SerializeField] protected Vector2 speakerNameOffset = new Vector2(-2f, 0f);
		[Required][SerializeField] protected Vector2 IconOffset = new Vector2(2f, 2f);
		[Required][SerializeField] protected Vector2 minBackgroundSize = new Vector2(1.2f, 1f);
		[Required][SerializeField] protected float minTextSize = 0f;
		[Required][SerializeField] protected float maxTextSize = 12f;
		[Required][SerializeField] protected string fallbackSpeakerName = "Disembodied Voice";
		[Required][SerializeField] protected bool growWithText = true;

		[Header("Debug")]
		[Readonly][SerializeField] protected bool skipToEnd;
		[Readonly][SerializeField] protected string currentText;
		[Readonly][SerializeField] protected internal float currentProgress;
		[Readonly][SerializeField] protected internal Speaker<SpeakerType> currentSpeaker;
		[Readonly][SerializeField] protected internal DialogEntry currentEntry;
		[Readonly][SerializeField] protected internal DialogContext currentContext;
		[Readonly][SerializeField] protected internal ChatBubbleInfo chatBubbleInfo;
		[Readonly][SerializeField] protected DialogOption currentDialogOption;

		public virtual void SoftReset(Transform newPosition = null) {
			if (newPosition != null) {
				this.transform.position = newPosition.position;
			}

			this.skipToEnd = false;
			this.currentText = "";
			this.currentEntry = null;
            this.currentProgress = 0f;
			this.currentContext = null;
			this.currentSpeaker = null;
			this.chatBubbleInfo = null;

            this.dialogText.text = "";
            this.speakerText.text = "";
            this.container.transform.localPosition = Vector3.zero;
			this.iconSpriteRenderer.gameObject.transform.localPosition = Vector3.zero;
			this.DoHide();
		}

		public virtual void OnSpawn(object spawner) {
			this.SoftReset();
            this.chatBubbleInfo = DialogManagerBase<SpeakerType>.instance.fallbackChatBubbleInfo;
            DialogManagerBase<SpeakerType>.instance.EventUpdateBubblePosition.AddListener(this.UpdatePosition);
		}

		public virtual void OnDespawn(object spawner) {
            DialogManagerBase<SpeakerType>.instance.EventUpdateBubblePosition.RemoveListener(this.UpdatePosition);
		}

        public virtual void DoShow() {
			this.dialogText.gameObject.SetActive(true);
			this.speakerText.gameObject.SetActive(true);
			this.iconSpriteRenderer.gameObject.SetActive(true);
			this.backgroundSpriteRenderer.gameObject.SetActive(true);
		}

		public virtual void DoHide() {
			this.iconSpriteRenderer.gameObject.SetActive(false);
			this.dialogText.gameObject.SetActive(false);
			this.speakerText.gameObject.SetActive(false);
			this.backgroundSpriteRenderer.gameObject.SetActive(false);
		}

		protected virtual bool UpdateSpeaker() {
			Speaker<SpeakerType> newSpeaker = DialogManagerBase<SpeakerType>.instance.LookupSpeaker(this.currentEntry);
			if (newSpeaker == null) {
				Debug.LogError($"Cannot find speaker in scene; {this.currentEntry.Speaker.DisplayName}");
				return false;
			}

			return UpdateSpeaker(newSpeaker);
		}

		protected virtual bool UpdateSpeaker(Speaker<SpeakerType> newSpeaker) {
			if (newSpeaker == null) {
				Debug.LogError($"No speaker given");
				return false;
			}

			this.speakerText.text = this.UpdateSpeaker_getSpeakerName(newSpeaker);
			this.speakerText.ForceMeshUpdate(); // Ensure text renders this frame so we can get the size

			if (!this.UpdateSpeaker_updateChatBubbleInfo(newSpeaker)) {
				return false;
			}

			this.currentSpeaker = newSpeaker;
			return true;
		}

		protected virtual string UpdateSpeaker_getSpeakerName(Speaker<SpeakerType> newSpeaker) {
			if ((this.currentEntry != null) && !string.IsNullOrEmpty(this.currentEntry.Speaker.DisplayName)) {
				return this.currentEntry.Speaker.DisplayName;
			}

			return this.fallbackSpeakerName;
		}

		protected virtual bool UpdateSpeaker_updateChatBubbleInfo(Speaker<SpeakerType> newSpeaker) {
			if ((newSpeaker == null) || !newSpeaker.IsDifferent(this.currentSpeaker)) {
				return true;
			}

			this.chatBubbleInfo = DialogManagerBase<SpeakerType>.instance.chatBubbleInfo.GetValueOrDefault(newSpeaker.speakerType, DialogManagerBase<SpeakerType>.instance.fallbackChatBubbleInfo);
			this.backgroundSpriteRenderer.sprite = this.chatBubbleInfo.background;
			return this.UpdateIcon(default);
		}

		protected virtual bool UpdateIcon(DialogOption newOption) {
			this.currentDialogOption = newOption;
			return this.UpdateIcon();
		}

		protected virtual bool UpdateIcon() {
			if (this.chatBubbleInfo == null) {
				Debug.Log("Cannot update icon");
				return false;
			}

			this.iconSpriteRenderer.sprite = this.chatBubbleInfo.iconSprite.GetValueOrDefault(this.currentDialogOption, this.chatBubbleInfo.fallbackIconSprite);

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

		protected virtual Vector2 UpdatePosition_getBackgroundPosition(Vector2 basePosition, Vector2 backgroundSize, ChatBubbleAlignment alignment) {
			switch (alignment) {
				case ChatBubbleAlignment.TopLeft:
					return basePosition + new Vector2(-backgroundSize.x / 2, backgroundSize.y / 2);

				case ChatBubbleAlignment.TopMiddle:
					return basePosition + new Vector2(0, backgroundSize.y / 2);

				case ChatBubbleAlignment.TopRight:
					return basePosition + new Vector2(backgroundSize.x / 2, backgroundSize.y / 2);

				case ChatBubbleAlignment.BottomLeft:
					return basePosition + new Vector2(-backgroundSize.x / 2, -backgroundSize.y / 2);

				case ChatBubbleAlignment.BottomMiddle:
					return basePosition + new Vector2(0, -backgroundSize.y / 2);

				case ChatBubbleAlignment.BottomRight:
					return basePosition + new Vector2(backgroundSize.x / 2, -backgroundSize.y / 2);

				case ChatBubbleAlignment.Left:
					return basePosition + new Vector2(-backgroundSize.x / 2, 0);

				case ChatBubbleAlignment.Right:
					return basePosition + new Vector2(backgroundSize.x / 2, 0);

				case ChatBubbleAlignment.Center:
					// no need to modify the position
					return basePosition;
			}

			Debug.Log($"Unknown alignment {alignment}");
			return basePosition;
		}

		protected virtual Vector2 UpdatePosition_getIconPosition(Vector2 basePosition, Vector2 backgroundSize, Vector2 iconSize, Vector2 offsetPosition) {
			// Move icon to bottom right corner
			return basePosition + new Vector2(backgroundSize.x / 2 - iconSize.x / 2 + offsetPosition.x, -backgroundSize.y / 2 + iconSize.y / 2 + offsetPosition.y);
		}

		protected virtual Vector2 UpdatePosition_getSpeakerPosition(Vector2 basePosition, Vector2 backgroundSize, Vector2 offsetPosition) {
			// Move name to top left corner
			return basePosition + new Vector2(-backgroundSize.x / 2 + 1 + offsetPosition.x, backgroundSize.y / 2 + offsetPosition.y);
		}

		public virtual IEnumerator Populate(DialogContext dialogContext, DialogEntry dialogEntry) {
			this.currentEntry = dialogEntry;
			this.currentContext = dialogContext;

			if (!UpdateSpeaker()) {
				yield break;
			}

			yield return this.Show();

			this.skipToEnd = false;
            yield return this.Populate_PrepareText(dialogEntry);
			yield return this.Populate_DisplayOverTime(dialogEntry);
			this.Populate_Finished();
			this.skipToEnd = false;
		}

		protected virtual IEnumerator Populate_PrepareText(DialogEntry dialogEntry) {
			if (this.currentText != "") {
                float initialProgress = this.currentProgress;
                yield return this.doOverTime(this.initialAdjustSizeSpeed, (progress) => this.UpdateTextProgress(initialProgress * (1 - progress)));
            }

			this.currentText = dialogEntry.Text;
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

		protected virtual void Populate_Finished() {
			this.UpdateTextProgress(1);
			this.UpdateIcon(this.currentContext.HasMatchingRule(this.currentEntry.ID) ? DialogOption.NextEntry : DialogOption.Close);
		}

		protected virtual IEnumerator Populate_DisplayOverTime(DialogEntry dialogEntry) {
			this.currentProgress = 0;
			this.UpdateIcon(DialogOption.SkipPopulate);
			float duration = this.currentText.Length / (this.charsPerSecond * dialogEntry.Speed);
			yield return this.doOverTime(duration, this.UpdateTextProgress);
		}

		internal virtual void UpdateTextProgress(float progress) {
			this.currentProgress = progress;
			this.UpdateTextProgress_PrepareText();

			if (this.growWithText) {
				this.backgroundSpriteRenderer.size = this.UpdateTextProgress_getBackgroundSize();
				this.UpdatePosition();
			}
		}

		protected virtual void UpdateTextProgress_PrepareText() {
			int textLength = Mathf.Max(0, Mathf.FloorToInt(this.currentText.Length * this.currentProgress));
			if (this.growWithText) {
				this.dialogText.text = this.currentText[..textLength];
				this.dialogText.ForceMeshUpdate(); // Ensure text renders this frame so we can get the size
			}
			else {
				// See: https://github.com/aarthificial-unity/foundations/blob/7d43e288317085920a55ea61c09bf30f3371b33c/Assets/View/Dialogue/DialogueBubble.cs#L122
				this.dialogText.maxVisibleCharacters = textLength;
			}
		}

		protected virtual Vector2 UpdateTextProgress_getBackgroundSize() {
			Vector2 textSize = this.dialogText.GetRenderedValues(false);
			Vector2 targetSize = (textSize + this.padding) / this.backgroundSpriteRenderer.transform.localScale;
			return new Vector2(Mathf.Max(this.minBackgroundSize.x, targetSize.x), Mathf.Max(this.minBackgroundSize.y, targetSize.y)); // Ensure the sprite does not get squished
		}

		protected virtual IEnumerator doOverTime(float duration, Action<float> OnProgressMade) {
			if (duration <= 0) {
				OnProgressMade(1);
				yield break;
			}

			float startTime = Time.time;

			float progress = 0;
			while (progress < 1) {
				float timeElapsed = Time.time - startTime;
				progress = Mathf.Clamp01(timeElapsed / duration);
				OnProgressMade(progress);
				yield return null;

				if (this.skipToEnd) {
					break;
				}
			}
		}

		protected virtual IEnumerator LerpOpacity(float initialOpacity, float targetOpacity, Action<float> extraLerp=null) {
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

        public virtual IEnumerator HideThenDespawn() {
            yield return this.Hide();
            DialogManagerBase<SpeakerType>.chatBubbleSpawner.Despawn(this);
        }

        public virtual void OnSkipToEnd() {
			this.skipToEnd = true;
		}

		public virtual bool HasAnotherEvent() {
			if (this.currentContext == null) {
				return false;
			}

			if (this.currentEntry == null) {
				return false;
			}

			return this.currentContext.HasMatchingRule(this.currentEntry.ID);
		}
	}
}