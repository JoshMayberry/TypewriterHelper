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
		[Required][SerializeField] protected float updatesPerSecond = 10f;
		[Required][SerializeField] protected float charsPerSecond = 32f;
		[Required][SerializeField] protected Vector2 padding = new Vector2(2f, 2f);
		[Required][SerializeField] protected Vector2 speakerNameOffset = new Vector2(-2f, 0f);
		[Required][SerializeField] protected Vector2 IconOffset = new Vector2(2f, 2f);
		[Required][SerializeField] protected Vector2 minBackgroundSize = new Vector2(0.36f, 0.32f);
		[Required][SerializeField] protected string fallbackSpeakerName = "Disembodied Voice";
		[Required][SerializeField] protected bool growWithText = true;

		[Header("Debug")]
		[Readonly][SerializeField] protected bool skipToEnd;
		[Readonly][SerializeField] protected internal float currentProgress;
		[Readonly][SerializeField] protected string currentText;
		[Readonly][SerializeField] protected internal Speaker<SpeakerType> currentSpeaker;
		[Readonly][SerializeField] protected internal DialogEntry currentEntry;
		[Readonly][SerializeField] protected internal DialogContext currentContext;
		[Readonly][SerializeField] protected internal ChatBubbleInfo chatBubbleInfo;
		[Readonly][SerializeField] protected DialogOption currentDialogOption;

		public virtual void SoftReset(Transform newPosition = null) {
			if (newPosition != null) {
				this.transform.position = newPosition.position;
			}

			this.currentContext = null;
			this.currentSpeaker = null;

			this.container.transform.localPosition = Vector3.zero;
			this.iconSpriteRenderer.gameObject.transform.localPosition = Vector3.zero;
			this.iconSpriteRenderer.gameObject.SetActive(true);
			this.backgroundSpriteRenderer.gameObject.SetActive(false);
			this.speakerText.gameObject.SetActive(false);
			this.dialogText.gameObject.SetActive(false);
		}

		public virtual void OnSpawn(object spawner) {
			DialogManagerBase<SpeakerType>.instance.EventUpdateBubblePosition.AddListener(this.UpdatePosition);
		}

		public virtual void OnDespawn(object spawner) {
			DialogManagerBase<SpeakerType>.instance.EventUpdateBubblePosition.RemoveListener(this.UpdatePosition);
		}

		public virtual void Show() {
			this.dialogText.gameObject.SetActive(true);
			this.speakerText.gameObject.SetActive(true);
			this.iconSpriteRenderer.gameObject.SetActive(true);
			this.backgroundSpriteRenderer.gameObject.SetActive(true);
		}

		public virtual void Hide(bool includeIcon = false) {
			if (includeIcon) {
				this.iconSpriteRenderer.gameObject.SetActive(false);
			}

			this.dialogText.gameObject.SetActive(false);
			this.speakerText.gameObject.SetActive(false);
			this.backgroundSpriteRenderer.gameObject.SetActive(false);
		}

		protected virtual bool UpdateSpeaker(Speaker<SpeakerType> newSpeaker = null) {
			string speakerName = "";
			if (newSpeaker == null) {
				speakerName = this.currentEntry.Speaker.DisplayName;
				newSpeaker = DialogManagerBase<SpeakerType>.instance.LookupSpeaker(this.currentEntry);

				if (newSpeaker == null) {
					Debug.LogError($"Cannot find speaker in scene; {speakerName}");
					return false;
				}
			}

			if (speakerName == "") {
				speakerName = newSpeaker.displayName;

				if ((speakerName == "") || (speakerName == null)) {
					speakerName = this.fallbackSpeakerName;
				}
			}

			this.speakerText.text = speakerName;
			this.speakerText.ForceMeshUpdate(); // Ensure text renders this frame so we can get the size

			if (newSpeaker.IsDifferent(this.currentSpeaker)) {
				this.chatBubbleInfo = DialogManagerBase<SpeakerType>.instance.chatBubbleInfo.GetValueOrDefault(newSpeaker.speakerType, DialogManagerBase<SpeakerType>.instance.fallbackChatBubbleInfo);

				this.backgroundSpriteRenderer.sprite = this.chatBubbleInfo.background;
				if (!this.UpdateIcon()) {
					return false;
				}
			}

			this.currentSpeaker = newSpeaker;
			return true;
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

			Vector2 upscaledSize = this.backgroundSpriteRenderer.size * this.backgroundSpriteRenderer.transform.localScale;

			Vector2 newPosition = this.currentSpeaker.chatBubblePosition.position;
			switch (this.currentSpeaker.chatBubbleAlignment) {
				case ChatBubbleAlignment.TopLeft:
					newPosition += new Vector2(-upscaledSize.x / 2, upscaledSize.y / 2);
					break;
				case ChatBubbleAlignment.TopMiddle:
					newPosition += new Vector2(0, upscaledSize.y / 2);
					break;
				case ChatBubbleAlignment.TopRight:
					newPosition += new Vector2(upscaledSize.x / 2, upscaledSize.y / 2);
					break;
				case ChatBubbleAlignment.BottomLeft:
					newPosition += new Vector2(-upscaledSize.x / 2, -upscaledSize.y / 2);
					break;
				case ChatBubbleAlignment.BottomMiddle:
					newPosition += new Vector2(0, -upscaledSize.y / 2);
					break;
				case ChatBubbleAlignment.BottomRight:
					newPosition += new Vector2(upscaledSize.x / 2, -upscaledSize.y / 2);
					break;
				case ChatBubbleAlignment.Left:
					newPosition += new Vector2(-upscaledSize.x / 2, 0);
					break;
				case ChatBubbleAlignment.Right:
					newPosition += new Vector2(upscaledSize.x / 2, 0);
					break;
				case ChatBubbleAlignment.Center:
					// no need to modify the position
					break;
			}

			this.container.transform.position = newPosition;

			// Move icon to bottom right corner
			Vector2 iconSize = this.iconSpriteRenderer.size;
			Vector2 iconPosition = (Vector2)this.container.transform.position + new Vector2(upscaledSize.x / 2 - iconSize.x / 2 + this.IconOffset.x, -upscaledSize.y / 2 + iconSize.y / 2 + this.IconOffset.y);
			this.iconSpriteRenderer.transform.position = iconPosition;

			// Move name to top left corner
			Vector2 namePosition = (Vector2)this.container.transform.position + new Vector2(-upscaledSize.x / 2 + 1 + this.speakerNameOffset.x, upscaledSize.y / 2 + this.speakerNameOffset.y);
			this.speakerText.gameObject.transform.position = namePosition;
		}

		// See: https://github.com/aarthificial-unity/foundations/blob/7d43e288317085920a55ea61c09bf30f3371b33c/Assets/View/Dialogue/DialogueBubble.cs#L122
		internal virtual void UpdateTextProgress(float progress) {
			int textLength = Mathf.Max(0, Mathf.FloorToInt(this.currentText.Length * progress));
			if (this.growWithText) {
				this.dialogText.text = this.currentText[..textLength];
				this.dialogText.ForceMeshUpdate(); // Ensure text renders this frame so we can get the size
			}
			else {
				this.dialogText.maxVisibleCharacters = textLength;
			}

			Vector2 textSize = this.dialogText.GetRenderedValues(false);
			Vector2 targetSize = (textSize + this.padding) / this.backgroundSpriteRenderer.transform.localScale;
			Vector2 clampedSize = new Vector2(Mathf.Max(this.minBackgroundSize.x, targetSize.x), Mathf.Max(this.minBackgroundSize.y, targetSize.y));

			// Ensure the sprite does not get squished
			this.backgroundSpriteRenderer.size = clampedSize;

			this.UpdatePosition();
		}

		public virtual IEnumerator PopulateChatBubble(DialogContext dialogContext, DialogEntry dialogEntry) {
			this.currentEntry = dialogEntry;
			this.currentContext = dialogContext;

			if (!this.UpdateSpeaker()) {
				yield break;
			}

			this.currentText = dialogEntry.Text;

			if (this.growWithText) {
				this.dialogText.text = "";
				this.dialogText.maxVisibleCharacters = 99999;
			}
			else {
				this.dialogText.text = this.currentText;
				this.dialogText.ForceMeshUpdate(); // Ensure text renders this frame so we can get the size
				this.dialogText.maxVisibleCharacters = 0;
			}


            this.Show();

			this.skipToEnd = false;
			float timeToWait = 1 / this.updatesPerSecond;
			this.currentProgress = 0;
			float startTime = Time.time;
			float duration = this.currentText.Length / (this.charsPerSecond * dialogEntry.Speed);
			this.UpdateIcon(DialogOption.SkipPopulate);
			while (this.currentProgress < 1) {
                if (this.skipToEnd) {
					break;
				}

                float timeElapsed = Time.time - startTime;
				this.currentProgress = Mathf.Clamp01(timeElapsed / duration);
				this.UpdateTextProgress(this.currentProgress);

				yield return new WaitForSeconds(timeToWait);
			}

			this.currentProgress = 1;
			this.UpdateTextProgress(1);

			this.UpdateIcon(this.currentContext.HasMatchingRule(this.currentEntry.ID) ? DialogOption.NextEntry : DialogOption.Close);
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