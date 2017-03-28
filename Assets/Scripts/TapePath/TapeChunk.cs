using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TapeChunk
{
	public bool isVisible;
	public readonly TapeObject owner;
	public readonly int chunkIndex;

	public GameObject gameObject { get; private set; }

	public Mesh mesh;

	public readonly Bounds nonTransformedBounds;

	public Bounds transformedBounds
	{
		get
		{
			var go = gameObject;
			var transform = go != null ? go.transform : owner.transform;
			return new Bounds (
				transform.TransformPoint (nonTransformedBounds.center),
				transform.TransformPoint (nonTransformedBounds.extents));
		}
	}

	public TapeChunk (TapeObject owner, int chunkIndex)
	{
		this.owner = owner;
		this.chunkIndex = chunkIndex;
		this.nonTransformedBounds = owner.tapeBounds.GetChunkBounds(chunkIndex);
	}

	internal void OnInitialize ()
	{
		mesh = new Mesh ();

		var chunkLength = owner.tapeBounds.chunkLength;

		Extrude (owner.tapeShape, owner.tapePath, chunkIndex * chunkLength, chunkLength + 1, mesh);
		mesh.RecalculateBounds ();

		isVisible = true;
		if (Application.isPlaying) {

			gameObject = new GameObject (typeof(TapeChunk).Name + " " + chunkIndex);

			gameObject.transform.SetParent (this.owner.transform);

			var renderer = gameObject.AddComponent<MeshRenderer> ();
			renderer.material.color = (chunkIndex & 1) != 0 ? Color.red : Color.blue;

			var meshFilter = gameObject.AddComponent<MeshFilter> ();

			meshFilter.sharedMesh = mesh;
		}
	}

	internal void OnDestroy ()
	{
		isVisible = false;

		if (gameObject != null) {
			MonoBehaviour.Destroy (gameObject);
			gameObject = null;
		}

		if (mesh != null) {
			if (Application.isPlaying)
				MonoBehaviour.Destroy (mesh);
			else
				MonoBehaviour.DestroyImmediate (mesh);
			mesh = null;
		}
	}

	public static void Extrude (TapeShape shape, TapePath path, int pathStart, int pathLength, Mesh mesh)
	{
		var lines = shape.lines;
		var shapeVertices = shape.shapeVertices; 
		var shapeUCoords = shape.shapeUCoords;

		int segments = pathLength - 1;
		int edgeLoops = pathLength;
		int vertCount = shapeVertices.Length * edgeLoops;
		int triCount = lines.Length * segments;
		int triIndexCount = triCount * 3;

		var triangles = new int[triIndexCount];
		var vertices = new Vector3[vertCount];
		var normals = new Vector3[vertCount];
		var uv = new Vector2[vertCount];

		for (int i = 0; i < pathLength; i++) {
			int offset = i * shapeVertices.Length;
			for (int j = 0; j < shapeVertices.Length; j++) {
				var point = path.GetPoint (pathStart + i);

				int id = offset + j;
				vertices [id] = point.LocalToWorld (shapeVertices [j]);
				normals [id] = point.LocalToWorldDirection (normals [j]);
				uv [id] = new Vector2 (shapeUCoords [j], i / ((float)edgeLoops));
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
}

