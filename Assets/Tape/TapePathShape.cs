using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent(typeof(TapePath))]
public class TapePathShape : CoreBehaviour
{
	private Vector3[] _shapeVertices;
	private Vector3[] _shapeNormals;
	private float[] _shapeUCoords;
	private int[] _shapeLines;
	private Vector3 _shapeMin;
	private Vector3 _shapeMax;
	private TapePath _tapePath;

	private bool _tapeRayRadiusValid;
	private Vector3 _tapeRayRadius;

	[Range(2, 100)]
	public int maxChunksToRender = 18;

	[Range(10, 800)]
	public int verticesPerChunk = 80;

	public TapePathShape() {
		BuildShape ();
	}

	public Vector3 tapeRayRadius {
		get { 
			if (!_tapeRayRadiusValid)
				UpdateTapeRayRadius ();
			return _tapeRayRadius; 
		}
	}

	public bool GetChunksRange (Transform transform, Camera camera, out int i1, out int i2)
	{
		var frustum = GeometryUtility.CalculateFrustumPlanes (camera);

		var tapePath = GetComponent<TapePath> ();
		float mind, maxd;
		var tTapeRay = transform.TransformRay (tapePath.tapeRay);
		if (!MathUtils.GetFrustumLineIntersection (frustum, tTapeRay, tapeRayRadius, out mind, out maxd)) {
			i1 = 0;
			i2 = 0;
			return false;
		}

		var chunkDepth = tapePath.cellDepth * TapeConstants.CellsPerChunk;

		float d = 1.0f / chunkDepth;
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
			var cameraRay = camera.ViewportPointToRay (new Vector3 (0.5F, 0.5F, camera.nearClipPlane));
			var tapeCameraDot = Vector3.Dot (cameraRay.direction, tTapeRay.direction);

			if (tapeCameraDot > 0) { // Camera is nearer to min
				i2 = i1 + maxChunksToRender;
			} else { // Camerar is nearer to max
				i1 = i2 - maxChunksToRender;
			}
		}

		return true;
	}

	public void Extrude (int chunkIndex, Mesh mesh)
	{
		var lines = _shapeLines;
		var shapeNormals = _shapeNormals;
		var shapeVertices = _shapeVertices; 
		var shapeUCoords = _shapeUCoords;

		int segments = verticesPerChunk - 1;
		int edgeLoops = verticesPerChunk;
		int vertCount = shapeVertices.Length * edgeLoops;
		int triCount = lines.Length * segments;
		int triIndexCount = triCount * 3;

		var triangles = new int[triIndexCount];
		var vertices = new Vector3[vertCount];
		var normals = new Vector3[vertCount];
		var uv = new Vector2[vertCount];

		var tapePath = this.GetComponent<TapePath> ();

		var chunkDepth = tapePath.cellDepth * TapeConstants.CellsPerChunk;

		for (int i = 0; i < verticesPerChunk; i++) {
			int offset = i * shapeVertices.Length;
			for (int j = 0; j < shapeVertices.Length; j++) {

				var relativeZ = i / (float)(verticesPerChunk - 1);
				var point = tapePath.GetRayPoint (chunkIndex * chunkDepth + relativeZ * chunkDepth);

				int id = offset + j;
				vertices [id] = point.LocalToWorld (shapeVertices [j]);
				normals [id] = point.LocalToWorldDirection (shapeNormals [j]);
				uv [id] = new Vector2 (shapeUCoords [j], relativeZ);
			}
		}

		int ti = 0;
		for (int i = 0; i < segments; i++) {
			int offset = i * shapeVertices.Length;
			for (int l = 0; l < lines.Length; l += 2) {
				int a = offset + lines [l] + shapeVertices.Length;
				int b = offset + lines [l];
				int c = offset + lines [l + 1];
				int d = offset + lines [l + 1] + shapeVertices.Length;
				triangles [ti++] = c;
				triangles [ti++] = b;
				triangles [ti++] = a;
				triangles [ti++] = a;
				triangles [ti++] = d;
				triangles [ti++] = c;
			}
		}

		mesh.Clear ();
		mesh.vertices = vertices;
		mesh.normals = normals;
		mesh.uv = uv;
		mesh.triangles = triangles;
	}

	protected override void OnCoreBehaviourValidate(CoreBehaviour behaviour) {
		if (behaviour == this || behaviour is TapePath) {
			_tapeRayRadiusValid = false;
		}
	}

	private void UpdateTapeRayRadius() {
		var deformationScale = GetComponent<TapePath>().deformationScale;

		var minX = _shapeMin.x * deformationScale.x;
		var minY = _shapeMin.y * deformationScale.y;
		var maxX = _shapeMax.x * deformationScale.x;
		var maxY = _shapeMax.y * deformationScale.y;

		var xRadius = Mathf.Abs ((maxX - minX) * deformationScale.x);
		var yRadius = Mathf.Abs ((maxY - minY) * deformationScale.y);

		_tapeRayRadius = new Vector3 (xRadius, yRadius, Mathf.Max (xRadius, yRadius));
		_tapeRayRadiusValid = true;
	}

	private void BuildShape() {
		_shapeVertices = new Vector3[] {

			new Vector3 (-5, 1, 0),
			new Vector3 (5, 1, 0),

			new Vector3 (5, 1, 0),

			new Vector3 (5, -1, 0),
			new Vector3 (-5, -1, 0),
			new Vector3 (-5, 1, 0)
		};

		_shapeNormals = new Vector3[] {

			new Vector3 (0, 1, 0),
			new Vector3 (0, 1, 0),

			new Vector3 (0, -1, 0),

			new Vector3 (-1, 0, 0),
			new Vector3 (0, -1, 0),
			new Vector3 (0, -1, 0)
		};

		_shapeUCoords = new float[] {
			0f, 1f, 0f, 1f, 0f, 1f
		};

		_shapeLines = Enumerable
			.Range (0, _shapeVertices.Length - 1)
			.SelectMany (i => LineSegment (i))
			.ToArray ();

		_shapeVertices.GetMinMax (out _shapeMin, out _shapeMax);
	}

	private static IEnumerable<int> LineSegment (int i)
	{
		yield return i;
		yield return i + 1;
	}
}

