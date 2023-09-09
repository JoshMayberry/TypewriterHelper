using System;
using Aarthificial.Typewriter.Entries;
using UnityEngine;

namespace jmayberry.TypewriterHelper.Entries {
    /// <summary>
    /// A rule representing a line of dialogue.
    /// </summary>
    /// <remarks>
    /// Typewriter entries can be extended to store additional information specific
    /// to your game.
    /// </remarks>
    [Serializable]
    public class DialogueArrayEntry : RuleEntry {
        /// <summary>
        /// The speed at which the text is revealed.
        /// </summary>
        [Range(0.25f, 2)] public float Speed = 1f;

        /// <summary>
        /// Whether this line should end with a choice.
        /// </summary>
        public bool IsChoice;

        /// <summary>
        /// Multiple lines of dialogue that will be played one after another.
        /// </summary>

        [SerializeField] public DialogueLineList Lines = new() { List = Array.Empty<DialogueArrayEntryLine>() };

        [Serializable]
        public struct DialogueLineList {
            public DialogueArrayEntryLine[] List;
        }
    }
}