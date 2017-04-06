using System;
using UnityEngine;

public class CoreBehaviour : MonoBehaviour
{
	public virtual void OnValidate() {
		var neighbours = gameObject.GetComponents<CoreBehaviour> ();
		for (var i = 0; i < neighbours.Length; ++i) {
			neighbours [i].OnCoreBehaviourValidate (this);
		}
	}

	protected virtual void OnCoreBehaviourValidate(CoreBehaviour behaviour) {
	}

	protected T GetComponent<T>(ref T field) 
		where T : Component
	{
		if (field != null)
			return field;
		field = GetComponent<T> ();
		return field;
	}
}

