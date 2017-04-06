using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TapeChunk: MonoBehaviour
{
	private bool _tapeMemoryValid;
	internal int _visibilityUpdateCount;
	internal float _visibilityLastTime;

	public Material material { get; private set; }
	public Mesh mesh { get; private set; }
	public TapeChunks owner { get; private set; }
	public int chunkIndex { get; private set; }

	internal void OnInitialize (TapeChunks owner, int chunkIndex)
	{
		_tapeMemoryValid = false;

		this.owner = owner;
		this.chunkIndex = chunkIndex;

		var meshFilter = this.GetComponent<MeshFilter> ();
		var renderer = this.GetComponent<MeshRenderer> ();

		mesh = meshFilter.sharedMesh;
		if (mesh == null)
			meshFilter.sharedMesh = mesh = new Mesh ();

		owner.tapePathShape.Extrude (chunkIndex, mesh);
		mesh.RecalculateBounds ();

		var originalRenderer = owner.GetComponent<MeshRenderer> ();
		material = new Material (originalRenderer.sharedMaterial);

		renderer.sharedMaterial = material;
		renderer.sortingLayerID = originalRenderer.sortingLayerID;
		renderer.sortingLayerName = originalRenderer.sortingLayerName;
		renderer.sortingOrder = originalRenderer.sortingOrder;
		renderer.reflectionProbeUsage = originalRenderer.reflectionProbeUsage;
		renderer.receiveShadows = originalRenderer.receiveShadows;
		renderer.probeAnchor = originalRenderer.probeAnchor;
		renderer.motionVectorGenerationMode = originalRenderer.motionVectorGenerationMode;
		renderer.shadowCastingMode = originalRenderer.shadowCastingMode;
	}

	public void OnTapeChanged() {
		_tapeMemoryValid = false;
	}

	public void Update() {
		if (!_tapeMemoryValid && material != null) {
			var tape = owner.Tape;

			var values = new float[TapeConstants.CellsPerChunk];
			if (tape != null) {
				for (int i = 0; i < values.Length; ++i)
					values [i] = TapeConstants.IndexOfSymbol(tape.GetCell(chunkIndex * TapeConstants.CellsPerChunk + i));
			}
			material.SetFloatArray ("_TapeMemory", values);
			_tapeMemoryValid = true;
		}
	}

	private void OnDestroy ()
	{
		material.SafeDestroy ();
		material = null;

		mesh.SafeDestroy ();
		mesh = null;
	}
}

