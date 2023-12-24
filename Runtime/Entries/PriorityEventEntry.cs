using Aarthificial.Typewriter.Entries;

using jmayberry.EventSequencer;

namespace jmayberry.TypewriterHelper.Entries {
	/// <summary>
	/// A higher priority allows an event to interrupt a currently running event
	/// </summary>
	public class PriorityEventEntry : EventEntry {
		public EventPriority priority;
	}
}