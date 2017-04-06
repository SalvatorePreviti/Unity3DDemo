using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
[RequireComponent (typeof(TapePath))]
[RequireComponent (typeof(TapePathShape))]
[RequireComponent (typeof(MeshRenderer))]
public class TapeChunks : CoreBehaviour
{
	private readonly Dictionary<int, TapeChunk> _chunks;
	private bool _chunksValid;
	private int _updateCounter;
	private TapePath _tapePath;
	private TapePathShape _tapePathShape;
	private Tape _tape;

	public TapeChunks() {
		_chunks = new Dictionary<int, TapeChunk> ();
	}

	public Tape Tape {
		get{ return _tape ?? (Tape = new Tape()); }
		set {
			var old = _tape;

			if (old != value) {
				if (old != null)
					old.TapeChanged -= OnTapeChanged;

				if (value != null)
					value.TapeChanged += OnTapeChanged;

				_tape = value;
				OnTapeChanged (null);
			}
		}
	}

	public TapePath tapePath {
		get {
			return _tapePath ?? gameObject.GetComponent (ref _tapePath);
		}
	}

	public TapePathShape tapePathShape {
		get {
			return _tapePathShape ?? gameObject.GetComponent (ref _tapePathShape);
		}
	}

	public Camera GetCamera ()
	{
		if (Application.isPlaying)
			return Camera.main;
		return Camera.current ?? Camera.main;
	}

	public void OnTapeChanged (int? index)
	{
		if (!index.HasValue) {
			foreach (var chunk in _chunks.Values)
				chunk.OnTapeChanged ();
		} else {
			TapeChunk chunk;
			_chunks.TryGetValue (index.Value / TapeConstants.CellsPerChunk, out chunk);
			if (chunk != null)
				chunk.OnTapeChanged ();
		}
	}

	void OnEnable ()
	{
		DestroyAllChunks ();
	}

	protected override void OnCoreBehaviourValidate(CoreBehaviour behaviour) {
		if (behaviour == this || behaviour is TapePath || behaviour is TapePathShape) {
			_chunksValid = false;
		}
	}

	public void OnTapeChanged(int index) {
		
	}

	void OnDrawGizmos ()
	{
		UpdateChunks ();
		foreach (var chunk in this._chunks.Values)
			chunk.Update ();
	}

	void Update() {
		UpdateChunks ();
	}

	void OnDestroy() {
		DestroyAllChunks ();
		var tape = _tape;
		if (tape != null)
			tape.TapeChanged -= OnTapeChanged;
	}

	void OnDisable() {
		DestroyAllChunks ();
	}

	private void UpdateChunks ()
	{
		if (!_chunksValid) { // First time cleanup, useful to delete dead objects in editor mode.
			_chunksValid = true;
			DestroyAllChunks ();
		}

		var updateCounter = ++this._updateCounter;

		var mainCamera = GetCamera ();
		if (mainCamera == null)
			return; 

		var time = ObjectUtils.GetGlobalTime ();

		int i1, i2;
		if (tapePathShape.GetChunksRange (transform, mainCamera, out i1, out i2)) {
			for (int i = i1; i < i2; ++i) {

				TapeChunk chunk;
				_chunks.TryGetValue (i, out chunk);

				if (chunk != null && chunk.gameObject == null) {
					chunk.gameObject.SafeDestroy ();
					chunk = null;
				}

				if (chunk == null) {
					var childGameObject = new GameObject (typeof(TapeChunk).Name + " " + i);
					ObjectUtils.SetObjectAsDynamic (childGameObject);
					childGameObject.transform.SetParent (transform);
					chunk = childGameObject.AddComponent<TapeChunk> ();
					_chunks [i] = chunk;
					chunk.OnInitialize (this, i);
				}

				chunk._visibilityUpdateCount = updateCounter;
				chunk._visibilityLastTime = time;
			}
		}

		List<TapeChunk> chunksToDestroy = null;
		foreach (var chunk in _chunks.Values) {

			var shouldDestroy = false;

			if (chunk.gameObject == null) {
				shouldDestroy = true;
			} else if (chunk._visibilityUpdateCount != updateCounter) { // invisible
				var totalChunks = _chunks.Count - (chunksToDestroy != null ? chunksToDestroy.Count : 0);
				if (totalChunks > tapePathShape.maxChunksToRender + 1 || time - chunk._visibilityLastTime > 1)
					shouldDestroy = true;
			}

			if (shouldDestroy)
				(chunksToDestroy ?? (chunksToDestroy = new List<TapeChunk> ())).Add (chunk);
		}

		if (chunksToDestroy != null) {
			foreach (var chunk in chunksToDestroy) {
				_chunks.Remove (chunk.chunkIndex, chunk);
				if (chunk != null)
					chunk.gameObject.SafeDestroy ();
			}
		}
	}

	private void DestroyAllChunks ()
	{
		if (_chunks != null) {
			foreach (var chunk in _chunks.Values.ToArray()) {
				if (chunk != null)
					chunk.gameObject.SafeDestroy ();
			}
			_chunks.Clear ();
		}
		ObjectUtils.DestroyDynamicChildrenRecursive (gameObject);
	}

}

