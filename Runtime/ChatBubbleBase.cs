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
		[Required] [SerializeField] protected TextMeshPro dialogText;
		[Required] [SerializeField] protected TextMeshPro speakerText;
		[Required] [SerializeField] protected SpriteRenderer iconSpriteRenderer;
		[Required] [SerializeField] protected SpriteRenderer backgroundSpriteRenderer;
		[Required][SerializeField] protected Transform container;

		[Required] [SerializeField] protected float charsPerSecond = 32f;
		[Required] [SerializeField] protected Vector2 padding = new Vector2(2f, 2f);
		[Required] [SerializeField] protected Vector2 minBackgroundSize = new Vector2(0.36f, 0.32f);
		[Required] [SerializeField] protected string fallbackSpeakerName = "Disembodied Voice";

        [Header("Debug")]
		[Readonly] [SerializeField] protected Speaker<SpeakerType> currentSpeaker;
		[Readonly] [SerializeField] protected DialogEntry currentEntry;
		[Readonly] [SerializeField] protected DialogContext currentContext;
        [Readonly] [SerializeField] protected ChatBubbleInfo chatBubbleInfo;
		[Readonly] [SerializeField] protected DialogOption currentDialogOption;

        //[SerializeField] internal UnityEvent EventFinished;
        //[SerializeField] internal UnityEvent EventEnded;

        //[Readonly] [SerializeField] private bool isTextCompleted = true;
        //[Readonly] [SerializeField] internal bool canSkipToEnd = true;

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

        public virtual void Show() {
			this.dialogText.gameObject.SetActive(true);
			this.speakerText.gameObject.SetActive(true);
			this.iconSpriteRenderer.gameObject.SetActive(true);
			this.backgroundSpriteRenderer.gameObject.SetActive(true);
		}

		public virtual void Hide(bool includeIcon=false) {
			if (includeIcon) {
                this.iconSpriteRenderer.gameObject.SetActive(false);
            }

            this.dialogText.gameObject.SetActive(false);
            this.speakerText.gameObject.SetActive(false);
			this.backgroundSpriteRenderer.gameObject.SetActive(false);
        }

        protected virtual bool UpdateSpeaker(Speaker<SpeakerType> newSpeaker=null) {
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

		protected virtual bool UpdateIcon() {
			if (this.chatBubbleInfo != null) {
				Debug.Log("Cannot update icon");
				return false;
			}
            this.iconSpriteRenderer.sprite = this.chatBubbleInfo.iconSprite.GetValueOrDefault(this.currentDialogOption, this.chatBubbleInfo.fallbackIconSprite);

			return true;
        }

        public virtual void UpdatePosition() {
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
			Vector2 iconPosition = (Vector2)this.container.transform.position + new Vector2(upscaledSize.x / 2 - iconSize.x / 2, -upscaledSize.y / 2 + iconSize.y / 2);
			this.iconSpriteRenderer.transform.position = iconPosition;

			// Move name to top left corner
			Vector2 namePosition = (Vector2)this.container.transform.position + new Vector2(-upscaledSize.x / 2 + 1, upscaledSize.y / 2);
			this.speakerText.gameObject.transform.position = namePosition;
        }

        // See: https://github.com/aarthificial-unity/foundations/blob/7d43e288317085920a55ea61c09bf30f3371b33c/Assets/View/Dialogue/DialogueBubble.cs#L122
        internal void UpdateTextProgress(float progress) {
			this.dialogText.maxVisibleCharacters = Mathf.Max(0, Mathf.FloorToInt(this.dialogText.textInfo.characterCount * progress));

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

			//bool isLastDialog = !dialogContext.HasMatchingRule(dialogEntry.ID);

			this.speakerText.text = this.currentSpeaker?.displayName ?? dialogEntry.Speaker.DisplayName ?? this.fallbackSpeakerName;

            this.dialogText.text = dialogEntry.Text;
            this.dialogText.ForceMeshUpdate(); // Ensure text renders this frame so we can get the size
			this.dialogText.maxVisibleCharacters = 0;

			this.Show();

			float timeToWait = this.charsPerSecond * dialogEntry.Speed;
			float progress = 0;
			float startTime = Time.time;
			float duration = this.dialogText.textInfo.characterCount / (timeToWait);
			while (progress < 1) {
				yield return new WaitForSeconds(duration);

				// TODO: Have a way to skip to the end of the text

				float timeElapsed = Time.time - startTime;
				progress = Mathf.Clamp01(timeElapsed / duration);
				this.UpdateTextProgress(progress);
			}

			this.UpdateTextProgress(1);
		}

		//      private void Update() {
		//	if (!this.isTextCompleted) {
		//		return;
		//	}

		//	float timeElapsed = Time.time - this.startTime;
		//	float duration = this.text.Length / (this.charsPerSecond * this.speed);
		//	float progress = Mathf.Clamp01(timeElapsed / duration);

		//	if (progress >= 1) {
		//		this.FinishText();
		//		return;
		//	}

		//	int textLength = Mathf.Max(0, Mathf.FloorToInt(text.Length * progress));

		//	// WARNING: This is a very naive approach to revealing the text. I used it
		//	// so that you don't have to install `TextMeshPro` for this example.
		//	// In a real game, consider using something like `maxVisibleCharacters`:
		//	// https://docs.unity3d.com/Packages/com.unity.textmeshpro@4.0/api/TMPro.TMP_Text.html#TMPro_TMP_Text_maxVisibleCharacters
		//	this.SetContents(text[..textLength]);
		//}

		//internal void UpdatePosition() {
		//	Vector2 upscaledSize = this.backgroundSpriteRenderer.size * this.backgroundSpriteRenderer.transform.localScale;

		//	Vector2 newPosition = this.speaker.chatBubblePosition.position;
		//	switch (this.speaker.chatBubbleAlignment) {
		//		case ChatBubbleAlignment.TopLeft:
		//			newPosition += new Vector2(-upscaledSize.x / 2, upscaledSize.y / 2);
		//			break;
		//		case ChatBubbleAlignment.TopMiddle:
		//			newPosition += new Vector2(0, upscaledSize.y / 2);
		//			break;
		//		case ChatBubbleAlignment.TopRight:
		//			newPosition += new Vector2(upscaledSize.x / 2, upscaledSize.y / 2);
		//			break;
		//		case ChatBubbleAlignment.BottomLeft:
		//			newPosition += new Vector2(-upscaledSize.x / 2, -upscaledSize.y / 2);
		//			break;
		//		case ChatBubbleAlignment.BottomMiddle:
		//			newPosition += new Vector2(0, -upscaledSize.y / 2);
		//			break;
		//		case ChatBubbleAlignment.BottomRight:
		//			newPosition += new Vector2(upscaledSize.x / 2, -upscaledSize.y / 2);
		//			break;
		//		case ChatBubbleAlignment.Left:
		//			newPosition += new Vector2(-upscaledSize.x / 2, 0);
		//			break;
		//		case ChatBubbleAlignment.Right:
		//			newPosition += new Vector2(upscaledSize.x / 2, 0);
		//			break;
		//		case ChatBubbleAlignment.Center:
		//			// no need to modify the position
		//			break;
		//	}

		//	this.transform.position = newPosition;

		//	// Move icon to bottom right corner
		//	Vector2 iconSize = this.iconSpriteRenderer.size;
		//	Vector2 iconPosition = (Vector2)this.transform.position + new Vector2(upscaledSize.x / 2 - iconSize.x / 2, -upscaledSize.y / 2 + iconSize.y / 2);
		//	this.iconSpriteRenderer.transform.position = iconPosition;

		//	// Move name to top left corner
		//	Vector2 namePosition = (Vector2)this.transform.position + new Vector2(-upscaledSize.x / 2 + 1, upscaledSize.y / 2);
		//	this.speakerText.gameObject.transform.position = namePosition;
		//}

		//private void SetSpeaker(ISpeaker newSpeaker) {
		//	string speakerName = newSpeaker.displayName;
		//	speakerName = newSpeaker.displayName;

		//	if ((speakerName == "") || (speakerName == null)) {
		//		speakerName = "Unknown";
		//	}

		//	this.speakerText.text = speakerName;
		//	this.speakerText.ForceMeshUpdate(); // Ensure text renders this frame so we can get the size

		//	if (this.speaker?.speakerType != newSpeaker.speakerType) {
		//		this.backgroundSpriteRenderer.sprite = EventSequencer.instance.chatBubbleSprite[(int)newSpeaker.speakerType];
		//		this.iconSpriteRenderer.sprite = EventSequencer.instance.chatButtonSprite[(int)newSpeaker.speakerType];
		//	}

		//	this.speaker = newSpeaker;
		//}

		//private void SetContents(string text) {
		//	this.dialogText.text = text;
		//	this.dialogText.ForceMeshUpdate(); // Ensure text renders this frame so we can get the size
		//	Vector2 textSize = this.dialogText.GetRenderedValues(false);
		//	Vector2 targetSize = (textSize + this.padding) / this.backgroundSpriteRenderer.transform.localScale;
		//	Vector2 clampedSize = new Vector2(Mathf.Max(0.36f, targetSize.x), Mathf.Max(0.32f, targetSize.y));

		//	// Ensure the sprite does not get squished
		//	this.backgroundSpriteRenderer.size = clampedSize;

		//	this.UpdatePosition();
		//}
		//internal void Begin(string text, Speaker speaker) {
		//	this.startTime = Time.time;
		//	this.text = text;
		//	this.SetSpeaker(speaker);
		//	this.gameObject.SetActive(true);
		//	this.isTextCompleted = false;

		//	if (this.canSkipToEnd) {
		//              this.inputMapper.EventInteract.AddListener(this.SkipToEnd);
		//	}
		//}

		//internal void SkipToEnd() {
		//	this.FinishText();
		//}

		//internal void FinishText() {
		//	this.inputMapper.EventInteract.RemoveListener(this.SkipToEnd);
		//	this.inputMapper.EventInteract.AddListener(this.End);
		//	this.isTextCompleted = true;
		//	this.SetContents(this.text);
		//	this.EventFinished.Invoke();
		//}

		//internal void End() {
		//	this.inputMapper.EventInteract.RemoveListener(this.SkipToEnd);
		//	this.isTextCompleted = true;
		//	this.gameObject.SetActive(false);
		//	this.EventEnded.Invoke();
		//}

		public void OnSpawn(object spawner) { }

		public void OnDespawn(object spawner) { }
	}
}