using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using jmayberry.CustomAttributes;
using jmayberry.TypewriterHelper;

public class InputManager : MonoBehaviour, IInputs {
    UnityEvent IInputs.EventInteract => this.EventInteract;
    [Readonly] public UnityEvent EventInteract;

    public static InputManager instance { get; private set; }
    private void Awake() {
		if (instance != null) {
			Debug.LogError("Found more than one Input Manager in the scene.");
		}

		instance = this;
	}

	public void OnInteract() {
		this.EventInteract.Invoke();
	}
}
