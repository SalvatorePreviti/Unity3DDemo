using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtils
{

	public static Ray TransformRay(this Transform transform, Ray ray) {
		return new Ray (transform.TransformPoint(ray.origin), transform.TransformDirection(ray.direction));
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
			var direction = frustum[i].Raycast (ray, out d);

			if ((!direction && d == 0.0f))
				continue; // No intersection, ray is parallel to the plane

			if (frustum[i].GetDistanceToPoint (ray.origin) < 0)
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

		if (a < b) {
			d1 = a;
			d2 = b;
		} else {
			d1 = b;
			d2 = a;
		}

		// Final step, build a bounding box and see if the computed points in the ray are valid

		var p1 = ray.GetPoint (d1);
		var p2 = ray.GetPoint (d2);

		return GeometryUtility.TestPlanesAABB(frustum, new Bounds((p1 + p2) / 2, (p2 - p1) / 2));
	}
}
