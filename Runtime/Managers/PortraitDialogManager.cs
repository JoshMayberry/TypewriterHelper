using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jmayberry.TypewriterHelper {
    public class PortraitDialogManager<SpeakerType> : BaseDialogManager<SpeakerType> where SpeakerType : Enum {
        protected override BaseChatSequence<SpeakerType> SpawnDialogSequence() {
            throw new NotImplementedException();
        }
    }
}