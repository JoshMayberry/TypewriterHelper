using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace jmayberry.TypewriterHelper
{
	public interface IInputs {
		UnityEvent EventInteract { get; }
	}
}
