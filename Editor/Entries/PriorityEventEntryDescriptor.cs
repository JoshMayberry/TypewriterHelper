using Aarthificial.Typewriter.Editor.Descriptors;
using jmayberry.TypewriterHelper.Entries;

namespace jmayberry.TypewriterHelper.Editor.Descriptors {
	[CustomEntryDescriptor(typeof(PriorityEventEntry))]
	public class PriorityEventEntryDescriptor : EventEntryDescriptor {
		public override string Name => "Priority Event";
		public override string Color => "#8bc34a";
	}
}