using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace jmayberry.TypewriterHelper.Applications {
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

	public abstract class ChatBubbleBase : MonoBehaviour {
		[SerializeField] internal UnityEvent EventFinished;
		[SerializeField] internal UnityEvent EventEnded;
        internal UnityEvent _EventInteract;

        [SerializeField] private SpriteRenderer iconSpriteRenderer;
		[SerializeField] private SpriteRenderer backgroundSpriteRenderer;
		[SerializeField] private TextMeshPro dialogText;
		[SerializeField] private TextMeshPro speakerText;

		[Readonly] [SerializeField] private bool isTextCompleted = true;
		[Readonly] [SerializeField] internal bool canSkipToEnd = true;

		[Readonly] public float startTime;
		[Readonly] public float speed = 1f;
		[Readonly] public string text;
		[Readonly] public Speaker speaker;

		[SerializeField] private float charsPerSecond = 32f;
		[SerializeField] private Vector2 padding = new Vector2(2f, 2f);

		private void Awake() {
			this.gameObject.SetActive(false);
		}

		private void Update() {
			if (!this.isTextCompleted) {
				return;
			}

			float timeElapsed = Time.time - this.startTime;
			float duration = this.text.Length / (this.charsPerSecond * this.speed);
			float progress = Mathf.Clamp01(timeElapsed / duration);

			if (progress >= 1) {
				this.FinishText();
				return;
			}

			int textLength = Mathf.Max(0, Mathf.FloorToInt(text.Length * progress));

			// WARNING: This is a very naive approach to revealing the text. I used it
			// so that you don't have to install `TextMeshPro` for this example.
			// In a real game, consider using something like `maxVisibleCharacters`:
			// https://docs.unity3d.com/Packages/com.unity.textmeshpro@4.0/api/TMPro.TMP_Text.html#TMPro_TMP_Text_maxVisibleCharacters
			this.SetContents(text[..textLength]);
		}

		internal void UpdatePosition() {
			Vector2 upscaledSize = this.backgroundSpriteRenderer.size * this.backgroundSpriteRenderer.transform.localScale;

			Vector2 newPosition = this.speaker.chatBubblePosition.position;
			switch (this.speaker.chatBubbleAlignment) {
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

			this.transform.position = newPosition;

			// Move icon to bottom right corner
			Vector2 iconSize = this.iconSpriteRenderer.size;
			Vector2 iconPosition = (Vector2)this.transform.position + new Vector2(upscaledSize.x / 2 - iconSize.x / 2, -upscaledSize.y / 2 + iconSize.y / 2);
			this.iconSpriteRenderer.transform.position = iconPosition;

			// Move name to top left corner
			Vector2 namePosition = (Vector2)this.transform.position + new Vector2(-upscaledSize.x / 2 + 1, upscaledSize.y / 2);
			this.speakerText.gameObject.transform.position = namePosition;
		}

		private void SetSpeaker(Speaker newSpeaker) {
			string speakerName = newSpeaker.displayName;
			speakerName = newSpeaker.displayName;

			if ((speakerName == "") || (speakerName == null)) {
				speakerName = "Unknown";
			}

			this.speakerText.text = speakerName;
			this.speakerText.ForceMeshUpdate(); // Ensure text renders this frame so we can get the size

			if (this.speaker?.speakerType != newSpeaker.speakerType) {
				this.backgroundSpriteRenderer.sprite = EventSequencer.instance.chatBubbleSprite[(int)newSpeaker.speakerType];
				this.iconSpriteRenderer.sprite = EventSequencer.instance.chatButtonSprite[(int)newSpeaker.speakerType];
			}

			this.speaker = newSpeaker;
		}

		private void SetContents(string text) {
			this.dialogText.text = text;
			this.dialogText.ForceMeshUpdate(); // Ensure text renders this frame so we can get the size
			Vector2 textSize = this.dialogText.GetRenderedValues(false);
			Vector2 targetSize = (textSize + this.padding) / this.backgroundSpriteRenderer.transform.localScale;
			Vector2 clampedSize = new Vector2(Mathf.Max(0.36f, targetSize.x), Mathf.Max(0.32f, targetSize.y));

			// Ensure the sprite does not get squished
			this.backgroundSpriteRenderer.size = clampedSize;

			this.UpdatePosition();
		}
		internal void Begin(string text, Speaker speaker) {
			this.startTime = Time.time;
			this.text = text;
			this.SetSpeaker(speaker);
			this.gameObject.SetActive(true);
			this.isTextCompleted = false;

			if (this.canSkipToEnd) {
				InputManager.instance.EventInteract.AddListener(this.SkipToEnd);
			}
		}

		internal void SkipToEnd() {
			this.FinishText();
		}

		internal void FinishText() {
			InputManager.instance.EventInteract.RemoveListener(this.SkipToEnd);
			InputManager.instance.EventInteract.AddListener(this.End);
			this.isTextCompleted = true;
			this.SetContents(this.text);
			this.EventFinished.Invoke();
		}

		internal void End() {
			InputManager.instance.EventInteract.RemoveListener(this.SkipToEnd);
			this.isTextCompleted = true;
			this.gameObject.SetActive(false);
			this.EventEnded.Invoke();
		}

        internal abstract UnityEvent GetEventInteract();
    }
}