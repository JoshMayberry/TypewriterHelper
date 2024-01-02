using System;
using UnityEngine;
using UnityEngine.Assertions;

using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.Entries;
using Aarthificial.Typewriter.References;

using jmayberry.EventSequencer;

namespace jmayberry.TypewriterHelper.Entries {

	[Serializable]
    public abstract class BaseEmoteDialogEntry<EmotionType> : DialogEntry where EmotionType : Enum {
        public EmotionType emotion;
        public PortraitSide portraitSide;
    }
}
