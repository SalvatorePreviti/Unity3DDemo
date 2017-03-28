using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtils
{
	/// <summary>Removes an entry from a dictionary checking by key and value.</summary>
	public static bool Remove<K, V>(this Dictionary<K, V> dictionary, K key, V value) {
		return (dictionary as ICollection<KeyValuePair<K, V>>).Remove (new KeyValuePair<K,V> (key, value));
	}

	public static Ray TransformRay (this Transform transform, Ray ray)
	{
		return new Ray (transform.TransformPoint (ray.origin), transform.TransformDirection (ray.direction));
	}

	/// <summary>
	/// Computes the intersection points between a frustum and an infinite line.
	/// Finds the visible part of a segment in respect of a camera frustum.
	/// Returns false if the line is not visible at all.
	/// </summary>
	/// <example>
	/// var planes = GeometryUtility.CalculateFrustumPlanes(camera);
	/// if (GetFrustumLineIntersection(planes, ray, out d1, out d2)) {
	/// 	Gizmos.DrawLine(ray.GetPoint(d1), ray.GetPoint(d2));
	/// }
	/// </example>
	/// <param name="planes">Frustum planes array</param>
	/// <param name="ray">The ray to intersect</param>
	/// <param name="d1">Output, minimum distance from the ray origin</param>
	/// <param name="d1">Output, maximum distance the ray origin</param>
	/// <returns>True if the segment is visible, false if not.</returns>
	public static bool GetFrustumLineIntersection (Plane[] frustum, Ray ray, out float d1, out float d2)
	{
		var a = 0.0f;
		var aValid = false;

		var b = 0.0f;
		var bValid = false;

		for (int i = 0; i < frustum.Length; ++i) {

			float d;
			var direction = frustum [i].Raycast (ray, out d);

			if ((!direction && d == 0.0f))
				continue; // No intersection, ray is parallel to the plane

			if (frustum [i].GetDistanceToPoint (ray.origin) < 0)
				direction = !direction; // Invert sign to ignore ray orientation

			// Choose where this point belong (a or b) depending on the direction.
			if (!direction) {
				if (!aValid || d > a) {
					a = d;
					aValid = true;
				}
			} else if (!bValid || d < b) {
				b = d;
				bValid = true;
			}
		}

		if (!aValid || !bValid) {
			// No intersection found.
			d1 = 0.0f;
			d2 = 0.0f;
			return false;
		}

		// Sort distances

		if (a <= b) {
			d1 = a;
			d2 = b;
		} else {
			d1 = b;
			d2 = a;
		}

		// Final step, build a bounding box and see if the computed points in the ray are valid

		var p1 = ray.GetPoint (d1);
		var p2 = ray.GetPoint (d2);

		return GeometryUtility.TestPlanesAABB (frustum, new Bounds ((p1 + p2) / 2, (p2 - p1) / 2));
	}

	/// <summary>
	/// wo non-parallel lines which may or may not touch each other have a point on each line which are closest
	/// to each other. This function finds those two points. If the lines are not parallel, the function returns true, otherwise false.
	/// </summary>
	public static bool ClosestPointsOnTwoRays(Ray r1, Ray r2, out float pointOnR1, out float pointOnR2)
	{
		var a = Vector3.Dot (r1.direction, r1.direction);
		var b = Vector3.Dot (r1.direction, r2.direction);
		var e = Vector3.Dot (r2.direction, r2.direction);

		float d = a * e - b * b;

		if (d == 0.0f) {
			pointOnR1 = 0.0f;
			pointOnR2 = 0.0f;
			return false;
		}

		var r = r1.origin - r2.origin;
		var c = Vector3.Dot (r1.direction, r);
		var f = Vector3.Dot (r2.direction, r);

		pointOnR1 = (b * f - c * e) / d;
		pointOnR2 = (a * f - c * b) / d;

		return true;
	}
}
