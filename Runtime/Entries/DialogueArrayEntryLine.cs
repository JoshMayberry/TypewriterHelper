using Aarthificial.Typewriter.Attributes;
using Aarthificial.Typewriter.References;
using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace jmayberry.TypewriterHelper.Entries {

    [Serializable]
    public class DialogueArrayEntryLine {
        /// <summary>
        /// The dialogue line.
        /// </summary>
        [TextArea] public string Text;

        /// <summary>
        /// A helper method for resolving the speaker reference.
        /// </summary>
        public SpeakerEntry Speaker {
            get {
                var speaker = _speaker.GetEntry<SpeakerEntry>();
                Assert.IsNotNull(
                  speaker,
                  $"Invalid speaker ID ({_speaker.ID}) required by \"{this}\""
                );
                return speaker;
            }
        }

        /// <summary>
        /// The speaker saying this line.
        /// </summary>
        /// <remarks>
        /// We can use the <see cref="EntryFilterAttribute"/> to restrict the entries
        /// we can reference to a specific type.
        /// </remarks>
        [EntryFilter(BaseType = typeof(SpeakerEntry))]
        [SerializeField]
        private EntryReference _speaker;
    }
}