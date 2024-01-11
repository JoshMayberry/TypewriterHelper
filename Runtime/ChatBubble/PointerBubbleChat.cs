using System;
using System.Collections.Generic;
using UnityEngine;

using jmayberry.CustomAttributes;

namespace jmayberry.TypewriterHelper {
	[Serializable]
	public abstract class PointerBubbleChat<SpeakerType, EmotionType> : BaseObjectChat<SpeakerType, EmotionType> where SpeakerType : Enum where EmotionType : Enum {
		[Header("Pointer: Setup")]
		[Required][SerializeField] protected SpriteRenderer pointToSpeakerSpriteRenderer;

		[Header("Pointer: Point To Speaker")]
		[Required][SerializeField] protected bool pointToSpeaker = true;
		[Required][SerializeField] protected float ptsDistanceFromSpeaker = 0.4f;
		[Required][SerializeField] protected float ptsDistanceFromSides = 0.6f;

		public override void SoftReset(Transform newPosition = null) {
			this.pointToSpeakerSpriteRenderer.transform.localScale = Vector3.zero;
			this.pointToSpeakerSpriteRenderer.transform.eulerAngles = Vector3.zero;
			this.pointToSpeakerSpriteRenderer.transform.localPosition = Vector3.zero;

			base.SoftReset(newPosition);
		}

		public override void DoShow() {
			base.DoShow();
			this.pointToSpeakerSpriteRenderer.gameObject.SetActive(this.pointToSpeaker);
		}

		public override void DoHide() {
			base.DoHide();
			this.pointToSpeakerSpriteRenderer.gameObject.SetActive(false);
		}

		protected override bool UpdateSprites() {
			if (!base.UpdateSprites()) {
				return false;
			}
			
			this.pointToSpeakerSpriteRenderer.sprite = this.chatBubbleInfo.pointToSpeakerSprite.GetValueOrDefault(this.currentChatBubbleType, this.chatBubbleInfo.fallbackPointToSpeakerSprite);
			return true;
		}

		public override void UpdatePosition() {
			if (this.currentSpeaker == null) {
				return;
			}
			base.UpdatePosition();

			if (this.pointToSpeaker) {
				Vector2 backgroundSize = this.backgroundSpriteRenderer.size * this.backgroundSpriteRenderer.transform.localScale;
				var (trianglePosition, angle, flipSide) = UpdatePosition_getPointToSpeaker(this.currentSpeaker.chatBubblePosition.position, this.backgroundSpriteRenderer.transform.position, backgroundSize);
			 
				this.pointToSpeakerSpriteRenderer.transform.position = trianglePosition;
				this.pointToSpeakerSpriteRenderer.transform.eulerAngles = new Vector3(0, 0, angle);
				this.pointToSpeakerSpriteRenderer.transform.localScale = new Vector3(flipSide, 1, 1);
			}
		}

		protected virtual (Vector2, float, int) UpdatePosition_getPointToSpeaker(Vector2 speakerPosition, Vector2 backgroundPosition, Vector2 backgroundSize) {
			Vector2 trianglePosition = Vector2.zero;
			float rotationAngle = 0f;
			bool flipHorizontally = false;

			bool isSpeakerAbove = speakerPosition.y > backgroundPosition.y;
			bool isSpeakerRight = speakerPosition.x > backgroundPosition.x;

			if (isSpeakerAbove) {
				rotationAngle = 180f;
				flipHorizontally = !isSpeakerRight;

				float xPos = Mathf.Clamp(speakerPosition.x, backgroundPosition.x - backgroundSize.x / 2 + this.ptsDistanceFromSides, backgroundPosition.x + backgroundSize.x / 2 - this.ptsDistanceFromSides);
				trianglePosition = new Vector2(xPos, backgroundPosition.y + backgroundSize.y / 2);
			}
			else {
				flipHorizontally = isSpeakerRight;

				float xPos = Mathf.Clamp(speakerPosition.x, backgroundPosition.x - backgroundSize.x / 2 + this.ptsDistanceFromSides, backgroundPosition.x + backgroundSize.x / 2 - this.ptsDistanceFromSides);
				trianglePosition = new Vector2(xPos, backgroundPosition.y - backgroundSize.y / 2);
			}

			if (isSpeakerRight) {
				trianglePosition.x = Mathf.Max(trianglePosition.x - this.ptsDistanceFromSpeaker, backgroundPosition.x - backgroundSize.x / 2 + this.ptsDistanceFromSides);
			}
			else {
				trianglePosition.x = Mathf.Min(trianglePosition.x + this.ptsDistanceFromSpeaker, backgroundPosition.x + backgroundSize.x / 2 - this.ptsDistanceFromSides);
			}

			return (trianglePosition, rotationAngle, (flipHorizontally ? -1 : 1));
		}
	}
}