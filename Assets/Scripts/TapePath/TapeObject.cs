using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TapeObject : MonoBehaviour {

	public Vector3 tapePathScale = new Vector3 (14, 14, 2);

	public float tapePathFrequency = 0.2f;

	public int seed = 12345;

	public int chunkLength = 100;

	public int minChunksToRender = 4;

	public int maxChunksToRender = 12;


	public TapePath tapePath;
	public TapeShape tapeShape;
	public TapeBounds tapeBounds;

	private Dictionary<int, TapeChunk> _chunks;

	public TapeObject()
	{
		_chunks = new Dictionary<int, TapeChunk> ();
		tapeShape = TapeShape.CreateDefault ();
	}

	private void LoadProperties() {
		tapePath = new TapePath (tapePathScale, tapePathFrequency, seed);
		tapeBounds = new TapeBounds (tapeShape, tapePath, chunkLength, minChunksToRender, maxChunksToRender);
		DestroyAllChunks ();
	}

	void Awake() {
		LoadProperties ();
	}

	void OnValidate() {
		LoadProperties ();
	}

	void OnDrawGizmos() {

		UpdateChunks ();

		Gizmos.color = Color.yellow;

		foreach (var chunk in _chunks.Values) {
			var bounds = chunk.transformedBounds;
			Gizmos.DrawWireCube (bounds.center, bounds.size);
			Gizmos.DrawMesh (chunk.mesh, transform.position, transform.rotation, transform.localScale);
		}
	}
	 
	public Camera GetCamera() {
		if (Application.isPlaying)
			return Camera.main;
		return Camera.current ?? Camera.main;
	}

	void Update () {
		UpdateChunks ();
	}

	private void UpdateChunks() {
		var mainCamera = GetCamera();
		if (mainCamera == null)
			return; 

		var planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);

		List<TapeChunk> chunksToDestroy = null;

		foreach (var chunk in _chunks.Values) {
			chunk.isVisible = GeometryUtility.TestPlanesAABB (planes, chunk.transformedBounds);
			if (!chunk.isVisible) {
				if (chunksToDestroy == null)
					chunksToDestroy = new List<TapeChunk>();
				chunksToDestroy.Add (chunk);
			}
		}

		int i1, i2;
		if (tapeBounds.GetChunksRange (transform, mainCamera.transform.position, planes, out i1, out i2)) {
			for (int i = i1; i < i2; ++i) {

				TapeChunk chunk;
				_chunks.TryGetValue (i, out chunk);

				if (chunk == null) {
					chunk = new TapeChunk (this, i);
					chunk.OnInitialize ();
					_chunks [i] = chunk;
				} else {
					chunk.isVisible = true;
				}
			}
		}

		if (chunksToDestroy != null) {
			foreach (var chunk in chunksToDestroy) {

				if (_chunks.Count <= tapeBounds.minChunksToRender)
					break; // No need to destroy more.

				if (!chunk.isVisible) {
					chunk.OnDestroy ();
					_chunks.Remove (chunk.chunkIndex);
				}
			}
		}
	}

	void OnDestroy() {
		DestroyAllChunks ();
		_chunks = null;
	}

	private void DestroyAllChunks() {
		if (_chunks != null) {
			var chunks = _chunks.Values.ToArray ();
			foreach (var chunk in chunks)
				chunk.OnDestroy ();
		}
	}
}
