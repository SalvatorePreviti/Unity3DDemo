using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtils
{
	public static bool GetMinMax (this Vector3[] vertices, out Vector3 min, out Vector3 max)
	{
		if (vertices == null || vertices.Length == 0) {
			min = Vector3.zero;
			max = Vector3.zero;
			return false;
		}

		float minX, minY, minZ;
		var maxX = minX = vertices [0].x;
		var maxY = minY = vertices [0].y;
		var maxZ = minZ = vertices [0].z;
		for (int i = 1; i < vertices.Length; ++i) {
			if (minX < vertices [i].x)
				minX = vertices [i].x;
			if (maxX > vertices [i].x)
				maxX = vertices [i].x;
			if (minY < vertices [i].y)
				minY = vertices [i].y;
			if (maxY > vertices [i].y)
				maxY = vertices [i].y;
			if (minZ < vertices [i].z)
				minZ = vertices [i].z;
			if (maxZ > vertices [i].z)
				maxZ = vertices [i].z;
		}
		min.x = minX;
		min.y = minY;
		min.z = minZ;
		max.x = maxX;
		max.y = maxY;
		max.z = maxZ;
		return true;
	}

	/// <summary>Removes an entry from a dictionary checking by key and value.</summary>
	public static bool Remove<K, V> (this Dictionary<K, V> dictionary, K key, V value)
	{
		return (dictionary as ICollection<KeyValuePair<K, V>>).Remove (new KeyValuePair<K,V> (key, value));
	}

	public static Ray TransformRay (this Transform transform, Ray ray)
	{
		return new Ray (transform.TransformPoint (ray.origin), transform.TransformDirection (ray.direction));
	}

	public static bool GetFrustumLineIntersection (Plane[] frustum, Ray ray, Vector3 tolerance, out float d1, out float d2)
	{
		d1 = 0f;
		d2 = 0f;

		float d1Angle = 0f, d2Angle = 0f;
		bool d1Valid = false, d2Valid = false;
		for (int i = 0; i < frustum.Length; ++i) {

			// Find the angle between a frustum plane and the ray.
			var angle = Mathf.Abs (Vector3.Angle (frustum [i].normal, ray.direction) - 90f);
			if (angle < 2f)
				continue; // Ray almost parallel to the plane, skip the plane.

			if (angle < d1Angle && angle < d2Angle)
				continue; // The angle is smaller than a previous angle that was better, skip the plane.

			// Cast a ray onto the plane to find the distance from ray origin where it happens.
			// Compute also the direction the ray hits the plane, backward or forward (dir) ignoring the ray direction.
			float d;
			var dir = frustum [i].Raycast (ray, out d) ^ (frustum [i].GetDistanceToPoint (ray.origin) >= 0);

			// Update d1 or d2, depending on the direction.
			if (dir) {
				d1Angle = angle;
				if (!d1Valid || d > d1) { // Choose the maximum value
					d1 = d;
					d1Valid = true;
				}
			} else {
				d2Angle = angle;
				if (!d2Valid || d < d2) { // Choose the minimum value
					d2 = d;
					d2Valid = true;
				}
			}
		}

		if (!d1Valid || !d2Valid)
			return false; // Points are not valid.

		// Sort points

		if (d1 > d2) {
			var t = d1;
			d1 = d2;
			d2 = t;
		}

		// Check whether points are visible in the frustum.

		var p1 = ray.GetPoint (d1);
		var p2 = ray.GetPoint (d2);

		var bb = new Bounds ();
		bb.SetMinMax (Vector3.Min (p1, p2) - tolerance, Vector3.Max (p1, p2) + tolerance);

		return GeometryUtility.TestPlanesAABB (frustum, bb);
	}
}
