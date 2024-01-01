using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

using jmayberry.CustomAttributes;
using jmayberry.Spawner;
using jmayberry.TypewriterHelper.Entries;
using Aarthificial.Typewriter;

namespace jmayberry.TypewriterHelper {
	[Serializable]
	public abstract class BaseUiChat<SpeakerType> : BaseChat<SpeakerType> where SpeakerType : Enum {

	}
}
