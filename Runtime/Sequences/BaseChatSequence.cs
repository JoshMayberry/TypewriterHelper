using System;
using System.Linq;
using System.Collections;

using UnityEngine;
using UnityEngine.Events;

using Aarthificial.Typewriter;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.References;

using jmayberry.TypewriterHelper.Entries;
using jmayberry.CustomAttributes;
using jmayberry.EventSequencer;
using jmayberry.Spawner;

namespace jmayberry.TypewriterHelper {
	public abstract class BaseChatSequence<SpeakerType> : SequenceBase where SpeakerType : Enum {
		[Header("Base: Debug")]
		[Readonly] public EventEntry rootEntry;
		[Readonly] public BaseChat<SpeakerType> chatBubble;
		[Readonly] public BaseEntry currentEntry;
		[Readonly] public bool isStarted;
		[Readonly] public bool isFinished;

		public override string ToString() {
			return $"<DialogSequenceBase:{this.GetHashCode()}>";
		}

		public BaseChatSequence() : base() { }

		public override void OnDespawn(object spawner) {
			TypewriterDatabase.Instance.RemoveListener(this.HandleTypewriterEvent);
			BaseDialogManager<SpeakerType>.instance.EventUserInteractedWithDialog.RemoveListener(this.OnUserInteracted);
		}

		public virtual void Reset() {
			this.isStarted = false;
			this.isFinished = false;

			this.currentEntry = null;
		}

		public virtual void OnUserInteracted() {
			if (!this.isStarted) {
				Debug.LogError("Sequence has not started");
				return;
			}

			if (this.isFinished) {
				Debug.LogError("Sequence has already finished");
				return;
			}

			this.chatBubble.OnSkipToEnd();
		}

		public virtual bool CanStart() {
			if (this.isStarted) {
				Debug.LogError("Sequence has already started");
				return false;
			}

			if (this.isFinished) {
				Debug.LogError("Sequence has already finished");
				return false;
			}

			if (this.rootEntry == null) {
				Debug.LogError("Root Entry not yet set");
				return false;
			}

			return true;
		}

		// See: https://github.com/aarthificial-unity/foundations/blob/7d43e288317085920a55ea61c09bf30f3371b33c/Assets/Player/PlayerController.cs#L121-L138
		public override IEnumerator Start(IContext iContext, Action<SequenceBase> callback) {
			if (!this.CanStart()) {
				yield break;
			}

			if (iContext is not DialogContext dialogContext) {
				Debug.LogError($"Unknown context type {iContext}");
				yield break;
			}

			yield return this.Start_Pre(dialogContext);

			this.isStarted = true;
			TypewriterDatabase.Instance.AddListener(this.HandleTypewriterEvent);
			dialogContext.Process(this.rootEntry);

			BaseEntry previousEntry = this.rootEntry;
			while (!this.isFinished) {
				yield return new WaitUntil(() => previousEntry != this.currentEntry);

				previousEntry = this.currentEntry;
				yield return this.HandleCurrentEntry(dialogContext, this.currentEntry);

				if (!dialogContext.HasMatchingRule(previousEntry.ID)) {
					this.isFinished = true;
				}
			}

			yield return this.Start_Post(dialogContext);

			callback?.Invoke(this);
		}

		public abstract IEnumerator Start_Pre(DialogContext dialogContext);

		public abstract IEnumerator Start_Post(DialogContext dialogContext);

		protected virtual IEnumerator HandleCurrentEntry(DialogContext dialogContext, BaseEntry baseEntry) {
			if (baseEntry is DialogEntry dialogEntry) {
				yield return this.chatBubble.Populate(dialogContext, dialogEntry);
			}
			else {
				Debug.LogError($"Unknown entry type {baseEntry}");
			}

			// Be finished with the entry
			dialogContext.TryInvoke(baseEntry);
		}

		internal void HandleTypewriterEvent(BaseEntry entry, ITypewriterContext iContext) {
			if (iContext is not DialogContext dialogContext) {
				Debug.LogError($"Unknown context type {iContext}");
				return;
			}

			if (entry is RuleEntry ruleEntry) {
				BaseEntry entryToCheck = (this.currentEntry != null ? this.currentEntry : this.rootEntry);
				if (!ruleEntry.Triggers.List.Contains(entryToCheck.ID)) {
					Debug.LogWarning("A rule that does not belong to this sequence was triggered");
					BaseDialogManager<SpeakerType>.instance.orphanedEntries.Add(ruleEntry);
					return;
				}

				this.currentEntry = ruleEntry;
				return;
			}

			Debug.LogError($"Unknown entry type {entry}");
		}

		public virtual bool ShouldOverride(BaseEntry entry) {
			if (this.isFinished) {
				return false;
			}

			if (this.chatBubble == null) {
				return false;
			}

			if (entry is PriorityEventEntry priorityEventEntry) {
				if (!base.ShouldOverride(priorityEventEntry.priority)) {
					return false;
				}
			}

			return !this.chatBubble.HasAnotherEvent();
		}

		public override EventPriority GetCurrentEventPriority() {
			if (!this.isStarted) {
				if (this.rootEntry is PriorityEventEntry priorityEventEntry) {
					return priorityEventEntry.priority;
				}
			}

			if (this.currentEntry is DialogEntry dialogEntry) {
				return dialogEntry.priority;
			}

			return EventPriority.None;
		}

		public override IEnumerator OnCancel() {
			this.isFinished = true;
			yield return null;
		}
	}
}