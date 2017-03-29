using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TapeChunk
{
	internal int _visibilityUpdateCount;
	internal float _visibilityLastTime;

	public readonly TapeObject owner;
	public readonly int chunkIndex;

	public GameObject gameObject { get; private set; }

	public Mesh mesh;

	public TapeChunk (TapeObject owner, int chunkIndex)
	{
		this.owner = owner;
		this.chunkIndex = chunkIndex;
	}

	internal void OnInitialize ()
	{
		mesh = new Mesh ();

		var chunkLength = owner.tapeBounds.chunkLength;

		Extrude (owner.tapeShape, owner.tapePath, chunkIndex * chunkLength, chunkLength + 1, owner.tapeBounds.cellSize, mesh);
		mesh.RecalculateBounds ();

		gameObject = new GameObject (typeof(TapeChunk).Name + " " + chunkIndex);
		ObjectUtils.SetObjectAsDynamic (gameObject);

		gameObject.transform.SetParent (this.owner.transform);
		 
		var originalRenderer = this.owner.GetComponent<MeshRenderer> ();

		// Create the mesh renderer.
		var renderer = gameObject.AddComponent<MeshRenderer> ();
		ObjectUtils.SetObjectAsDynamic (renderer);

		// Copy relevant properties.

		renderer.sharedMaterial = originalRenderer.sharedMaterial;
		renderer.sharedMaterials = originalRenderer.sharedMaterials;
		renderer.sortingLayerID = originalRenderer.sortingLayerID;
		renderer.sortingLayerName = originalRenderer.sortingLayerName;
		renderer.sortingOrder = originalRenderer.sortingOrder;
		renderer.reflectionProbeUsage = originalRenderer.reflectionProbeUsage;
		renderer.receiveShadows = originalRenderer.receiveShadows;
		renderer.probeAnchor = originalRenderer.probeAnchor;
		renderer.motionVectorGenerationMode = originalRenderer.motionVectorGenerationMode;
		renderer.shadowCastingMode = originalRenderer.shadowCastingMode;

		var meshFilter = gameObject.AddComponent<MeshFilter> ();
		ObjectUtils.SetObjectAsDynamic (meshFilter);

		meshFilter.sharedMesh = mesh;
	}
	 
	internal void OnDestroy ()
	{
		if (mesh != null) {
			if (Application.isPlaying)
				MonoBehaviour.Destroy (mesh);
			else
				MonoBehaviour.DestroyImmediate (mesh);
			mesh = null;
		}

		if (gameObject != null) {
			ObjectUtils.DestroyDynamicChildrenRecursive (gameObject);
			ObjectUtils.SafeDestroy (gameObject);
			gameObject = null;
		}
	}

	public static void Extrude (TapeShape shape, TapePath path, int pathStart, int pathLength, float cellSize, Mesh mesh)
	{
		var lines = shape.lines;
		var shapeNormals = shape.shapeNormals;
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
				normals [id] = point.LocalToWorldDirection (shapeNormals [j]);
				uv [id] = new Vector2 (shapeUCoords [j], i / cellSize);
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

