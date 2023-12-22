using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using jmayberry.TypewriterHelper;

public enum SpeakerType {
    Unknown,
    System,
    Slime,
    Skeleton,
}

public class DialogManager : DialogManagerBase<SpeakerType> {
}
