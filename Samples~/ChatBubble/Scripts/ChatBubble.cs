using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using jmayberry.TypewriterHelper.Applications;

public class ChatBubble : ChatBubbleBase {
    internal override UnityEvent GetEventInteract() {
        return InputManager.instance.EventInteract;
    }
}
