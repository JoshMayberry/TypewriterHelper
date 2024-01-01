using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using jmayberry.CustomAttributes;
using jmayberry.Spawner;
using jmayberry.TypewriterHelper.Entries;
using Aarthificial.Typewriter;

namespace jmayberry.TypewriterHelper {
	public enum ChatBubbleType {
		Normal,
		Thought,
		Yell,
		Quiet,
	}


	[Serializable]
	public abstract class BaseChat<SpeakerType> : MonoBehaviour, ISpawnable where SpeakerType : Enum {
		[Header("Object: Setup")]
		[Required][SerializeField] protected TMP_Text dialogText;
		[Required][SerializeField] protected TMP_Text speakerText;

		[Header("Base: Tweak")]
		[Required][SerializeField] protected Vector2 padding = new Vector2(2f, 2f);
		[Required][SerializeField] protected Vector2 minContainerSize = new Vector2(1.2f, 1f);
		[Required][SerializeField] protected float charsPerSecond = 32f;
		[Required][SerializeField] protected float initialAdjustSizeSpeed = 0.1f;
		[Required][SerializeField] protected string fallbackSpeakerName = "Disembodied Voice";
		[Required][SerializeField] protected bool growWithText = true;

		[Header("Base: Debug")]
		[Readonly][SerializeField] protected bool isUserInteracted;
		[Readonly][SerializeField] protected string currentText;
		[Readonly][SerializeField] protected internal float currentProgress;
		[Readonly][SerializeField] protected internal Speaker<SpeakerType> currentSpeaker;
		[Readonly][SerializeField] protected internal DialogEntry currentEntry;
		[Readonly][SerializeField] protected internal DialogContext currentContext;
		[Readonly][SerializeField] protected internal ChatBubbleInfo chatBubbleInfo;
		[Readonly][SerializeField] protected DialogOption currentDialogOption;
		[Readonly][SerializeField] protected ChatBubbleType currentChatBubbleType;
		[Readonly][SerializeField] protected UnitySpawner<BaseChat<SpeakerType>> spawner;

		public virtual void SoftReset(Transform newPosition = null) {
			if (newPosition != null) {
				this.transform.position = newPosition.position;
			}

			this.isUserInteracted = false;
			this.currentText = "";
			this.currentEntry = null;
			this.currentProgress = 0f;
			this.currentContext = null;
			this.currentSpeaker = null;
			this.chatBubbleInfo = null;

			this.dialogText.text = "";
			this.speakerText.text = "";

			this.DoHide();
		}

		public virtual void OnSpawn(object spawner) {
			if (spawner is UnitySpawner<BaseChat<SpeakerType>> sequenceSpawner) {
				this.spawner = sequenceSpawner;
			}
			else {
				Debug.LogError($"Unknown spawner type {spawner}");
			}

			this.SoftReset();
			this.chatBubbleInfo = BaseDialogManager<SpeakerType>.instance.fallbackChatBubbleInfo;
		}

		public virtual void OnDespawn(object spawner) {
			this.spawner = null;
		}

		public virtual void Despawn() {
			if (this.spawner != null) {
				this.spawner.Despawn(this);
			}
			else {
				Debug.LogError("Spawner not set");
			}
		}

		protected virtual bool UpdateSpeaker() {
			Speaker<SpeakerType> newSpeaker = BaseDialogManager<SpeakerType>.instance.LookupSpeaker(this.currentEntry);
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
			//this.speakerText.ForceMeshUpdate(); // Ensure text renders this frame so we can get the size

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

			this.chatBubbleInfo = BaseDialogManager<SpeakerType>.instance.chatBubbleInfo.GetValueOrDefault(newSpeaker.speakerType, BaseDialogManager<SpeakerType>.instance.fallbackChatBubbleInfo);
			return this.UpdateSprites(default);
		}

		protected virtual bool UpdateSprites(DialogOption newOption) {
			this.currentDialogOption = newOption;
			return this.UpdateSprites();
		}

		protected abstract bool UpdateSprites();

		public virtual IEnumerator Populate(DialogContext dialogContext, DialogEntry dialogEntry) {
            yield return this.Populate_PreLoop(dialogContext, dialogEntry);

            int i = 0;
			int i_lastOne = dialogEntry.TextList.Length - 1;
			bool isLastInSequence = !this.currentContext.HasMatchingRule(this.currentEntry.ID);

            foreach (string dialog in dialogEntry.TextList) {
				this.isUserInteracted = false;
				bool isLastInLoop = (i >= i_lastOne);
                yield return this.Populate_Loop(dialogContext, dialogEntry, dialog, i, isLastInLoop, isLastInSequence);

                this.isUserInteracted = false;
				yield return new WaitUntil(() => this.isUserInteracted);
				i++;
			}

			this.isUserInteracted = false;
		}

		protected internal virtual IEnumerator Populate_PreLoop(DialogContext dialogContext, DialogEntry dialogEntry) {
            this.currentEntry = dialogEntry;
            this.currentContext = dialogContext;
            this.currentChatBubbleType = dialogEntry.chatKind;

            if (!UpdateSpeaker()) {
                yield break;
            }

            yield return this.Show();
        }

		protected virtual IEnumerator Populate_PrepareText(string dialog) {
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
		}

		public virtual IEnumerator Populate_Loop(DialogContext dialogContext, DialogEntry dialogEntry, string dialog, int i, bool isLastInLoop, bool isLastInSequence) {
            yield return this.Populate_PrepareText(dialog);
            yield return this.Populate_DisplayOverTime(dialogEntry.Speed);
            this.Populate_Finished(isLastInLoop, isLastInSequence);
        }

		protected virtual void Populate_Finished(bool isLastInLoop, bool isLastInSequence) {
			this.UpdateTextProgress(1);
			this.UpdateSprites((isLastInLoop && isLastInSequence) ? DialogOption.Close : DialogOption.NextEntry);
		}

		protected virtual IEnumerator Populate_DisplayOverTime(float speed) {
			this.currentProgress = 0;
			this.UpdateSprites(DialogOption.SkipPopulate);
			float duration = this.currentText.Length / (this.charsPerSecond * speed);
			yield return this.doOverTime(duration, this.UpdateTextProgress);
		}

		internal virtual void UpdateTextProgress(float progress) {
			this.currentProgress = progress;
			this.UpdateTextProgress_PrepareText();

			if (this.growWithText) {
				this.UpdateContainerSize(this.GetContainerTargetSize());
			}
		}

		protected abstract Vector2 GetContainerTargetSize();

		protected abstract void UpdateContainerSize(Vector2 newSize);

		protected abstract Vector2 GetContainerScale();

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

		public virtual IEnumerator Show() {
			if (this.IsShown()) {
				yield break;
			}

			this.dialogText.text = "";
			this.DoShow();

			if (this.growWithText) {
				this.UpdateContainerSize(this.minContainerSize);
			}

			this.dialogText.ForceMeshUpdate(); // Ensure text renders this frame so we can get the size
			yield return this.LerpOpacity(0, 1);
		}

		public virtual IEnumerator Hide() {
			if (!this.IsShown()) {
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

		protected virtual bool IsShown() {
			return this.dialogText.gameObject.activeSelf;
		}

		protected internal virtual IEnumerator OnFinishedSequence() {
			yield return this.Hide();
		}

		protected internal virtual IEnumerator DespawnCoroutine() {
			yield return this.Hide();
			this.Despawn();
		}

		public virtual void DoShow() {
			this.dialogText.gameObject.SetActive(true);
			this.speakerText.gameObject.SetActive(true);
			this.SetSpriteActive(true);
		}

		public virtual void DoHide() {
			this.dialogText.gameObject.SetActive(false);
			this.speakerText.gameObject.SetActive(false);
			this.SetSpriteActive(false);
		}

		protected abstract void SetSpriteActive(bool state);

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

				if (this.isUserInteracted) {
					this.isUserInteracted = false;
					break;
				}
			}
		}

		protected abstract Color GetSpriteColor();
		protected abstract void SetSpriteColor(Color newColor);

		protected virtual IEnumerator LerpOpacity(float initialOpacity, float targetOpacity, Action<float> extraLerp = null) {
			Color dialog_originalColor = new Color(this.dialogText.color.r, this.dialogText.color.g, this.dialogText.color.b, initialOpacity);
			Color dialog_targetColor = new Color(dialog_originalColor.r, dialog_originalColor.g, dialog_originalColor.b, targetOpacity);

			Color speaker_originalColor = new Color(this.speakerText.color.r, this.speakerText.color.g, this.speakerText.color.b, initialOpacity);
			Color speaker_targetColor = new Color(speaker_originalColor.r, speaker_originalColor.g, speaker_originalColor.b, targetOpacity);

			Color spriteColor = GetSpriteColor();
			Color sprite_originalColor = new Color(spriteColor.r, spriteColor.g, spriteColor.b, initialOpacity);
			Color sprite_targetColor = new Color(sprite_originalColor.r, sprite_originalColor.g, sprite_originalColor.b, targetOpacity);

			yield return this.doOverTime(this.initialAdjustSizeSpeed, (float progress) => {
				this.dialogText.color = Color.Lerp(dialog_originalColor, dialog_targetColor, progress);
				this.speakerText.color = Color.Lerp(speaker_originalColor, speaker_targetColor, progress);
				this.SetSpriteColor(Color.Lerp(sprite_originalColor, sprite_targetColor, progress));
				extraLerp?.Invoke(progress);
			});
		}

		public virtual void SetOpacity(float value) {
			this.dialogText.color = new Color(this.dialogText.color.r, this.dialogText.color.g, this.dialogText.color.b, value);
			this.speakerText.color = new Color(this.speakerText.color.r, this.speakerText.color.g, this.speakerText.color.b, value);

			Color spriteColor = GetSpriteColor();
			this.SetSpriteColor(new Color(spriteColor.r, spriteColor.g, spriteColor.b, value));
		}

		public virtual void OnSkipToEnd() {
			this.isUserInteracted = true;
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
