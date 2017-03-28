using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]
public class TapeObject : MonoBehaviour {

	public Vector3 tapePathScale = new Vector3 (14, 14, 2);

	public float tapePathFrequency = 0.2f;

	public int seed = 12345;

	public int chunkLength = 100;

	public int minChunksToRender = 4;

	public int maxChunksToRender = 16;

	public TapePath tapePath;
	public TapeShape tapeShape;
	public TapeBounds tapeBounds;

	private Dictionary<int, TapeChunk> _chunks;
	private bool _needCleanup;

	public TapeObject()
	{
		_needCleanup = true;
		_chunks = new Dictionary<int, TapeChunk> ();
		tapeShape = TapeShape.CreateDefault ();
	}

	void OnEnable() {
		DestroyAllChunks ();
	}

	void OnDisable() {
		DestroyAllChunks ();
	}

	void Awake() {
		LoadProperties ();
	}

	/// <summary>Called by editor when some property changes or code is recompiled.</summary>
	void OnValidate() {
		LoadProperties ();
	}

	void OnDrawGizmos() {
		UpdateChunks ();
	}
	 
	public Camera GetCamera() {
		if (Application.isPlaying)
			return Camera.main;
		return Camera.current ?? Camera.main;
	}

	void Update () {
		UpdateChunks ();
	}
	 
	void OnDestroy() {
		DestroyAllChunks ();
	}

	private void LoadProperties() {
		DestroyAllChunks (true);
		tapePath = new TapePath (tapePathScale, tapePathFrequency, seed);
		tapeBounds = new TapeBounds (tapeShape, tapePath, chunkLength, minChunksToRender, maxChunksToRender);
	}

	private void UpdateChunks() {
		if (_needCleanup) { // First time cleanup, useful to delete dead objects in editor mode.
			_needCleanup = false;
			DestroyAllChunks ();
		}

		var mainCamera = GetCamera();
		if (mainCamera == null)
			return; 

		var time = Time.time;

		int i1, i2;
		if (tapeBounds.GetChunksRange (transform, mainCamera, out i1, out i2)) {
			for (int i = i1; i < i2; ++i) {

				TapeChunk chunk;
				_chunks.TryGetValue (i, out chunk);

				if (chunk != null && chunk.gameObject == null) {
					chunk.OnDestroy ();
					chunk = null;
				}

				if (chunk == null) {
					chunk = new TapeChunk (this, i);
					chunk.OnInitialize ();
					_chunks [i] = chunk;
				}

				chunk._visibilityUpdateTime = time;
			}
		}

		List<TapeChunk> chunksToDestroy = null;
		foreach (var chunk in _chunks.Values) {
			if (chunk.gameObject == null || (chunk._visibilityUpdateTime != time && _chunks.Count > minChunksToRender)) {
				if (chunksToDestroy == null)
					chunksToDestroy = new List<TapeChunk> ();
				chunksToDestroy.Add (chunk);
			}
		}

		if (chunksToDestroy != null) {
			foreach (var chunk in chunksToDestroy) {
				_chunks.Remove (chunk.chunkIndex, chunk);
				chunk.OnDestroy ();
			}
		}
	}

	public void DestroyAllChunks(bool deferred = false) {
		if (deferred) {
			_needCleanup = true;
		} else {
			if (_chunks != null)
				_chunks.Clear ();
			ObjectUtils.DestroyDynamicChildrenRecursive (gameObject);
		}
	}
}
