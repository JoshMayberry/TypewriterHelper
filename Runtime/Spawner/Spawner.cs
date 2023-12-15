using UnityEngine;
using System.Collections.Generic;

namespace jmayberry.TypewriterHelper {
public class UnitySpawner<T> where T : Component {
	[SerializeField] private T prefab;

	private List<T> activeList = new List<T>();
	private List<T> inactiveList = new List<T>();

	public UnitySpawner(T prefab) {
		this.prefab = prefab;
	}

	public T Spawn() {
		T spawnling;

		if (inactiveList.Count > 0) {
			spawnling = inactiveList[inactiveList.Count - 1];
			inactiveList.RemoveAt(inactiveList.Count - 1);
		}
		else {
			spawnling = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity);
		}

		activeList.Add(spawnling);
		return spawnling;
	}

	public void Despawn(T spawnling) {
		activeList.Remove(spawnling);
		inactiveList.Add(spawnling);
	}
}

public class CodeSpawner<T> where T : new() {
	private List<T> activeList = new List<T>();
	private List<T> inactiveList = new List<T>();

	public T Spawn() {
		T spawnling;

		if (inactiveList.Count > 0) {
			spawnling = inactiveList[inactiveList.Count - 1];
			inactiveList.RemoveAt(inactiveList.Count - 1);
		}
		else {
			spawnling = new T();
		}

		activeList.Add(spawnling);
		return spawnling;
	}

	public void Despawn(T spawnling) {
		activeList.Remove(spawnling);
		inactiveList.Add(spawnling);
	}
}
}