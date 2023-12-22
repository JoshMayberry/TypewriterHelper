using System;

using Aarthificial.Typewriter;
using Aarthificial.Typewriter.Blackboards;

using jmayberry.EventSequencer;

namespace jmayberry.TypewriterHelper {
	[Serializable]
	public class DialogContext : ITypewriterContext, IContext {
		private const int _globalScope = 1388552;
		private const int _contextScope = 1388553;

		private static readonly Blackboard _global = new();
		private readonly Blackboard _context = new();

		public virtual bool TryGetBlackboard(int scope, out IBlackboard blackboard) {
			switch (scope) {
				case _globalScope:
					blackboard = _global;
					return true;

				case _contextScope:
					blackboard = this._context;
					return true;

				default:
					blackboard = default;
					return false;
			}
		}
	}
}