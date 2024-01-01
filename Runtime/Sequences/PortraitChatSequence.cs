using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace jmayberry.TypewriterHelper {
    public class PortraitChatSequence<SpeakerType> : BaseChatSequence<SpeakerType> where SpeakerType : Enum {
        public override IEnumerator Start_Post(DialogContext dialogContext) {
            throw new NotImplementedException();
        }

        public override IEnumerator Start_Pre(DialogContext dialogContext) {
            throw new NotImplementedException();
        }
    }
}