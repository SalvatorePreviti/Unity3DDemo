using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapeBounds
{
	public readonly int chunkLength;
	public readonly int maxChunksToRender;


	public readonly Vector3 tapePathScale;

	public readonly Ray tapeRay;
	public readonly Vector3 tapeRayRadius;

	public readonly float cellSize;

	public TapeBounds (TapeShape tapeShape, TapePath tapePath, int chunkLength, int maxChunksToRender, float cellSize)
	{
		this.chunkLength = chunkLength;
		this.maxChunksToRender = maxChunksToRender;
		this.cellSize = cellSize;


		tapeRay = new Ray (Vector3.zero, Vector3.forward);

		var minX = tapeShape.minX - tapePath.Scale.x;
		var minY = tapeShape.minY - tapePath.Scale.y;
		var maxX = tapeShape.maxX + tapePath.Scale.x;
		var maxY = tapeShape.maxY + tapePath.Scale.y;

		var xRadius = Mathf.Abs (maxX - minX);
		var yRadius = Mathf.Abs (maxY - minY);

		tapeRayRadius = new Vector3 (xRadius, yRadius, Mathf.Max (xRadius, yRadius));

		tapePathScale = tapePath.Scale;
	}

	public bool GetChunksRange (Transform transform, Camera camera, out int i1, out int i2)
	{
		var cameraRay = camera.ViewportPointToRay (new Vector3 (0.5F, 0.5F, camera.nearClipPlane));
		var frustum = GeometryUtility.CalculateFrustumPlanes (camera);

		float mind, maxd;
		var tTapeRay = transform.TransformRay (tapeRay);
		if (!MathUtils.GetFrustumLineIntersection (frustum, tTapeRay, tapeRayRadius, out mind, out maxd)) {
			i1 = 0;
			i2 = 0;
			return false;
		}

		float d = 1.0f / (tapePathScale.z * chunkLength);
		i1 = Mathf.FloorToInt (mind * d - 0.1f);
		i2 = Mathf.CeilToInt (maxd * d + 0.1f);

		if (i2 < i1) {
			var tmp = i2;
			i2 = i1;
			i1 = tmp;
		}

		if (i2 == i1)
			return true;

		if (i2 - i1 > maxChunksToRender) { // Too many chunks, trim down.
			//var tTapeRay = transform.TransformRay (this.tapeRay);

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


