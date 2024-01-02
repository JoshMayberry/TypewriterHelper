using UnityEditor;

using jmayberry.TypewriterHelper.Entries;
using Aarthificial.Typewriter.Editor.Descriptors;
using jmayberry.TypewriterHelper.Editor.Descriptors;

namespace jmayberry.TypewriterHelper.Editor.Descriptors {
    public class BaseEmoteDialogEntryDescriptor : DialogEntryDescriptor {
		public override string Name => "Emote Dialogue";
        public override string Color => "#a5deea";
    }

    public class BaseEmoteDialogEntryPropertyDrawer : DialogEntryDescriptor { }
}