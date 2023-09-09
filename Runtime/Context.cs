using Aarthificial.Typewriter;
using Aarthificial.Typewriter.Blackboards;
using System;

namespace jmayberry.TypewriterHelper.Applications {
        /// <summary>
        /// A sample implementation of <see cref="ITypewriterContext"/>.
        /// </summary>
        /// <remarks>
        /// In this example, the context is made up of two blackboards. The global
        /// blackboard is shared between all contexts, while the context blackboard is
        /// local to the given conversation.
        /// </remarks>
        [Serializable]
	public class Context : ITypewriterContext {
		private const int _globalScope = 1388552;
		private const int _contextScope = 1388553;

		private static readonly Blackboard _global = new();
		private readonly Blackboard _context = new();

		public bool TryGetBlackboard(int scope, out IBlackboard blackboard) {
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