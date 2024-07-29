using UnityEngine;
using System.Collections;
using jmayberry.EventSequencer;
using jmayberry.TypewriterHelper.Entries;

public enum MyActionType {
	None,
	Spawn,
}

public class MyActionEntry : BaseActionEntry<MyActionType> {
	public override IEnumerator DoAction(IContext context) {
		switch (this.action) {
			case MyActionType.Spawn:
				Debug.Log("Would have spawned in something using a manager instance");
				break;
		}

		yield return null;
	}
}
