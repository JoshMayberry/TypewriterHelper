using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
		[Header("Base: Tweak")]
		[Required][SerializeField] protected float charsPerSecond = 32f;
		[Required][SerializeField] protected float initialAdjustSizeSpeed = 0.1f;
		[Required][SerializeField] protected string fallbackSpeakerName = "Disembodied Voice";

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
		}

		public abstract void OnSpawn(object spawner);
		public abstract void OnDespawn(object spawner);

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

			this.UpdateSpeaker_setText(newSpeaker);

			if (!this.UpdateSpeaker_updateChatBubbleInfo(newSpeaker)) {
				return false;
			}

			this.currentSpeaker = newSpeaker;
			return true;
		}

		protected abstract void UpdateSpeaker_setText(Speaker<SpeakerType> newSpeaker);

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
			return this.UpdateSprites(default);
		}

		protected virtual bool UpdateSprites(DialogOption newOption) {
			this.currentDialogOption = newOption;
			return this.UpdateSprites();
		}

		protected abstract bool UpdateSprites();

		public virtual IEnumerator Populate(DialogContext dialogContext, DialogEntry dialogEntry) {
			this.currentEntry = dialogEntry;
			this.currentContext = dialogContext;
			this.currentChatBubbleType = dialogEntry.chatKind;

			if (!UpdateSpeaker()) {
				yield break;
			}

			yield return this.Populate_PreLoop();

			int i = 0;
			int i_lastOne = dialogEntry.TextList.Length - 1;
			foreach (string dialog in dialogEntry.TextList) {
				this.isUserInteracted = false;
				yield return this.Populate_PrepareText(dialog);
				yield return this.Populate_DisplayOverTime(dialogEntry.Speed);
				this.Populate_Finished((i >= i_lastOne) && !this.currentContext.HasMatchingRule(this.currentEntry.ID));

				this.isUserInteracted = false;
				yield return new WaitUntil(() => this.isUserInteracted);
				i++;
			}

			this.isUserInteracted = false;
		}

		protected abstract IEnumerator Populate_PreLoop();

		protected abstract IEnumerator Populate_PrepareText(string dialog);

		protected virtual void Populate_Finished(bool isLast) {
			this.UpdateTextProgress(1);
			this.UpdateSprites(isLast ? DialogOption.Close : DialogOption.NextEntry);
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
		}

		protected virtual void UpdateTextProgress_PrepareText() {
			int textLength = Mathf.Max(0, Mathf.FloorToInt(this.currentText.Length * this.currentProgress));
			this.UpdateTextProgress_SetText(textLength);
		}

		protected abstract void UpdateTextProgress_SetText(int textLength);

		protected internal abstract IEnumerator OnFinishedSequence();
		protected internal abstract IEnumerator DespawnCoroutine();

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
