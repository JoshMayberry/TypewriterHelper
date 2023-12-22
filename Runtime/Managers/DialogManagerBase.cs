using System;
using System.Collections.Generic;
using UnityEngine;

using AYellowpaper.SerializedCollections;
using Aarthificial.Typewriter.References;
using Aarthificial.Typewriter.Tools;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter;

using jmayberry.TypewriterHelper.Entries;
using jmayberry.CustomAttributes;
using jmayberry.EventSequencer;
using jmayberry.Spawner;
using static UnityEngine.GraphicsBuffer;

namespace jmayberry.TypewriterHelper {

	public enum DialogOption {
		Unknown,
		NextEntry,
		SkipPopulate,
		Close,
		SelectOption,
	}

	[Serializable]
	public class  ChatBubbleInfo {
		public Sprite background;
		[SerializedDictionary("Dialog Option", "Icon")] public SerializedDictionary<DialogOption, Sprite> iconSprite;
	}

	public class DialogManagerBase : EventManagerBase {
		[Header("Setup")]
		[SerializedDictionary("Speaker Type", "Chat Bubble")] public SerializedDictionary<SpeakerType, ChatBubbleInfo> chatBubbleInfo;
		[Required][SerializeField] internal ChatBubbleBase chatBubblePrefab;

		[Header("For Debugging")]
		[SerializeField] internal static Dictionary<int, ISpeaker> speakerLookup;
		[Readonly] public static DialogContext defaultContext = new DialogContext();

        private TypewriterWatcher typewriterWatcher;
		private static UnitySpawner<ChatBubbleBase> chatBubbleSpawner;
        //internal CodeSpawner<DialogueSequenceBase<SpeakerType>> uiCardSpawner { get; private set; }
        internal CodeSpawner<EventSequenceBase> uiCardSpawner { get; private set; }

        public static DialogManagerBase<SpeakerType> instance { get; private set; }

		private void Awake() {
			if (instance != null && instance != this) {
				Debug.LogError("Found more than one DialogManager<SpeakerType> in the scene.");
				Destroy(this.gameObject);
				return;
			}

			instance = this;

			speakerLookup = new Dictionary<int, ISpeaker>();
		}
		private void OnEnable() {
			TypewriterDatabase.Instance.AddListener(this.HandleTypewriterEvent);
		}

		private void OnDisable() {
			TypewriterDatabase.Instance.RemoveListener(this.HandleTypewriterEvent);
		}

		public int GetFact(ITypewriterContext context, EntryReference factReference) {
			factReference.TryGetEntry(out FactEntry factEntry);
			return context.Get(factEntry);
		}

		public void SetFact(ITypewriterContext context, EntryReference factReference, int value, bool isSilent = false) {
			factReference.TryGetEntry(out FactEntry factEntry);
			context.Set(factEntry, value);

			if (!isSilent) {
				TypewriterDatabase.Instance.MarkChange();
			}
		}

		public void IncrementFact(ITypewriterContext context, EntryReference factReference, int value, bool isSilent = false) {
			factReference.TryGetEntry(out FactEntry factEntry);
			context.Add(factEntry, value);

			if (!isSilent) {
				TypewriterDatabase.Instance.MarkChange();
			}
		}

		public ISpeaker LookupSpeaker(DialogEntry currentEntry) {
			return speakerLookup[currentEntry.Speaker.ID];
		}

		internal ChatBubbleBase SpawnChatBubble() {
			return chatBubbleSpawner.Spawn();
		}

		internal void DespawnChatBubble(ChatBubbleBase chatBubble) {
			chatBubbleSpawner.Despawn(chatBubble);
		}

		internal void HandleTypewriterEvent(BaseEntry entry, ITypewriterContext iContext) {
			
		}
	}
}