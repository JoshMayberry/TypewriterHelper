using Aarthificial.Typewriter.Entries;

namespace Aarthificial.Typewriter.Editor.Descriptors {
    [CustomEntryDescriptor(typeof(PriorityEventEntry))]
    public class PriorityEventEntryDescriptor : EventEntryDescriptor {
        public override string Name => "Priority Event";
        public override string Color => "#8bc34a";
    }
}