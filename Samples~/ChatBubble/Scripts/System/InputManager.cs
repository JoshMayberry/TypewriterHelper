using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InputManager : MonoBehaviour {
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
