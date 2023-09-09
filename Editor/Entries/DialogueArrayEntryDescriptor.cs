using Aarthificial.Typewriter.Editor.Descriptors;
using jmayberry.TypewriterHelper.Entries;

namespace jmayberry.TypewriterHelper.Editor.Descriptors {
        [CustomEntryDescriptor(typeof(DialogueArrayEntry))]
    public class DialogueArrayEntryDescriptor : RuleEntryDescriptor {
        public override string Name => "Dialogue Array";

        public override string Color => "#2196f3";
    }
}