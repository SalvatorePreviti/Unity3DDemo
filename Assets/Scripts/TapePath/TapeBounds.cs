using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapeBounds
{
	public readonly int chunkLength;
	public readonly int maxChunksToRender;
	public readonly int minChunksToRender;

	public readonly float minX;
	public readonly float minY;
	public readonly float maxX;
	public readonly float maxY;

	public readonly Vector3 tapePathScale;

	public readonly Ray[] boundingRays;
	public readonly Ray tapeRay;

	public TapeBounds (TapeShape tapeShape, TapePath tapePath, int chunkLength, int minChunksToRender, int maxChunksToRender)
	{
		this.chunkLength = chunkLength;
		this.maxChunksToRender = maxChunksToRender;

		minX = tapeShape.minX - tapePath.Scale.x;
		minY = tapeShape.minY - tapePath.Scale.y;
		maxX = tapeShape.maxX + tapePath.Scale.x;
		maxY = tapeShape.maxY + tapePath.Scale.y;

		tapePathScale = tapePath.Scale;

		boundingRays = new Ray[] {
			new Ray (new Vector3 (minX, minY, 0), Vector3.forward),
			new Ray (new Vector3 (maxX, minY, 0), Vector3.forward),
			new Ray (new Vector3 (maxX, maxY, 0), Vector3.forward),
			new Ray (new Vector3 (minX, maxY, 0), Vector3.forward)
		};

		tapeRay = new Ray (Vector3.zero, Vector3.forward);
	}

	public Bounds GetChunkBounds (int chunkIndex)
	{
		float dMultiplier = tapePathScale.z * chunkLength;
		float d0 = chunkIndex * dMultiplier;
		float d1 = (chunkIndex + 1) * dMultiplier;

		Vector3 min = new Vector3 (float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 max = new Vector3 (float.MinValue, float.MinValue, float.MinValue);

		var boundingRays = this.boundingRays;
		for (var i = 0; i < boundingRays.Length; ++i) {

			var p1 = boundingRays [i].GetPoint (d0);
			var p2 = boundingRays [i].GetPoint (d1);

			min = Vector3.Min (min, Vector3.Min (p1, p2));
			max = Vector3.Max (max, Vector3.Max (p1, p2));
		}

		return new Bounds ((max + min) / 2.0f, (max - min) * 2.0f);
	}

	public bool GetChunksRange (Transform transform, Vector3 cameraPosition, Plane[] frustum, out int i1, out int i2)
	{
		var isVisible = false;
		float mind = float.MaxValue, maxd = float.MinValue;
		foreach (var ray in boundingRays) {

			var transformedRay = transform.TransformRay (ray);

			float d1, d2;
			if (MathUtils.GetFrustumLineIntersection (frustum, transformedRay, out d1, out d2)) {
				isVisible = true;
				mind = Mathf.Min (d1, mind);
				maxd = Mathf.Max (d2, maxd);
			}
		}

		if (!isVisible) {
			i1 = 0;
			i2 = 0;
			return false;
		}

		float d = 1.0f / (tapePathScale.z * chunkLength);
		i1 = Mathf.FloorToInt (mind * d);
		i2 = Mathf.CeilToInt (maxd * d);

		if (i2 - i1 > maxChunksToRender) {

			var transformedTapeRay = transform.TransformRay (tapeRay);
			var pd1 = Vector3.Distance (transformedTapeRay.GetPoint (mind), cameraPosition);
			var pd2 = Vector3.Distance (transformedTapeRay.GetPoint (maxd), cameraPosition);

			if (pd1 <= pd2) { // Camera is nearer to min
				i2 = i1 + maxChunksToRender;
			} else { // Camerar is nearer to max
				i1 = i2 - maxChunksToRender;
			}
		}

		return true;
	}
}


