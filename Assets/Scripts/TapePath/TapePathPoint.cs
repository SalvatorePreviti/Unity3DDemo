using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TapePathPoint {

	public Vector3 position;
	public Quaternion rotation;

	public TapePathPoint(Vector3 position, Vector3 tangent)
	{
		this.position = position;
		var binormal = Vector3.Cross(Vector3.up, tangent).normalized;
		var normal = Vector3.Cross(tangent, binormal);
		this.rotation = Quaternion.LookRotation(tangent, normal);
	}

	public Vector3 LocalToWorld(Vector3 point) {
		return position + rotation * point;
	}

	public Vector3 WorldToLocal(Vector3 point) {
		return Quaternion.Inverse(rotation) * (point - position);
	}

	public Vector3 LocalToWorldDirection(Vector3 direction) {
		return rotation * direction;
	}
}
