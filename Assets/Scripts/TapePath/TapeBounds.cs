using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapeBounds
{
	public readonly int chunkLength;
	public readonly int maxChunksToRender;

	public readonly float minX;
	public readonly float minY;
	public readonly float maxX;
	public readonly float maxY;

	public readonly Vector3 tapePathScale;

	public readonly Ray[] boundingRays;
	public readonly Ray tapeRay;

	public TapeBounds (TapeShape tapeShape, TapePath tapePath, int chunkLength, int maxChunksToRender)
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

	public bool GetChunksRange (Transform transform, Camera camera, out int i1, out int i2)
	{
		var isVisible = false;
		float mind = float.MaxValue, maxd = float.MinValue;

		var frustum = GeometryUtility.CalculateFrustumPlanes(camera);

		foreach (var ray in boundingRays) {
			float d1, d2;
			if (MathUtils.GetFrustumLineIntersection (frustum, transform.TransformRay (ray), out d1, out d2)) {
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

		if (i2 < i1) {
			var tmp = i2;
			i2 = i1;
			i1 = tmp;
		}

		if (i2 == i1)
			return true;

		if (i2 - i1 > maxChunksToRender) // Too many chunks, trim down.
		{
			var tTapeRay = transform.TransformRay (this.tapeRay);
			var cameraRay = camera.ViewportPointToRay (new Vector3 (0.5F, 0.5F, 0));

			var tapeCameraDot = Vector3.Dot (cameraRay.direction, tTapeRay.direction);

			if (tapeCameraDot > 0) { // Camera is nearer to min
				i2 = i1 + maxChunksToRender;
			} else { // Camerar is nearer to max
				i1 = i2 - maxChunksToRender;
			}
		}

		return true;
	}
}


