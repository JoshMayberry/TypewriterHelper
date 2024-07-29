using System;
using Aarthificial.Typewriter.Editor.Descriptors;

namespace jmayberry.TypewriterHelper.Editor.Descriptors {
    public class BaseActionEntryDescriptor<ActionType> : RuleEntryDescriptor where ActionType : Enum {
		public override string Name => "Action";
		public override string Color => "#9cdbe8";
	}
}
