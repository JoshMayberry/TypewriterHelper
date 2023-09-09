using Aarthificial.Typewriter.Editor.Descriptors;
using Aarthificial.Typewriter.Entries;

[CustomEntryDescriptor(typeof(PriorityEventEntry))]
public class PriorityEventEntryDescriptor : EventEntryDescriptor {
    public override string Name => "Priority Event";
    public override string Color => "#8bc34a";
}