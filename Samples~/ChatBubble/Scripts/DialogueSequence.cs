using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using jmayberry.TypewriterHelper.Applications;

public class DialogueSequence : DialogueSequenceBase {
    internal DialogueSequence() : base() {
    }

    internal override UnityEvent GetEventInteract() {
        return InputManager.instance.EventInteract;
    }
}
