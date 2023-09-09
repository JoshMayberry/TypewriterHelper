using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Aarthificial.Typewriter.Entries {
    /// <summary>
    /// A higher priority allows an event to interrupt a currently running event
    /// </summary>
    public class PriorityEventEntry : EventEntry {
        public int priority = 0;
    }
}
