using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Aarthificial.Typewriter.Entries;

namespace jmayberry.TypewriterHelper.Entries {
        /// <summary>
        /// A higher priority allows an event to interrupt a currently running event
        /// </summary>
        public class PriorityEventEntry : EventEntry {
        public int priority = 0;
    }
}
