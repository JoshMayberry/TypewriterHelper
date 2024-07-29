using System;
using System.Collections;

using Aarthificial.Typewriter.Entries;
using jmayberry.EventSequencer;

namespace jmayberry.TypewriterHelper.Entries {
	[Serializable]
	public abstract class BaseActionEntry<ActionType> : RuleEntry where ActionType : Enum {
		public EventPriority priority = EventPriority.None;
		public ActionType action;

		public abstract IEnumerator DoAction(IContext context);
	}
}