using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[ExecuteInEditMode]
public class TapePath : CoreBehaviour
{
	[Range (1, 15)]
	public int termsCount = 5;

	public int randomSeed = 123456;

	public Vector2 deformationScale = new Vector2 (30f, 12f);

	public float deformationFrequency = 0.025f;

	[Range(0.0001f, 100f)]
	public float cellDepth = 10;

	private Vector2[] _frequencies;
	private Vector2[] _phases;

	public readonly Ray tapeRay = new Ray(Vector3.zero, Vector3.forward);

	public void Awake ()
	{
		Reload ();
	}

	public override void OnValidate() {
		Reload ();
		base.OnValidate ();
	}

	public Vector3 GetRayPosition (float t)
	{
		var frequencies = _frequencies;
		var phases = _phases;

		float px = 0, py = 0;
		var ft = t * deformationFrequency;
		for (int i = 0; i < frequencies.Length; ++i) {
			px += Mathf.Sin (ft * frequencies [i].x + phases [i].x);
			py += Mathf.Sin (ft * frequencies [i].y + phases [i].y);
		}

		px *= deformationScale.x / frequencies.Length;
		py *= deformationScale.y / frequencies.Length;

		return new Vector3 (px, py, t);
	}

	public Vector3 GetRayTangent (float t)
	{
		var frequencies = _frequencies;
		var phases = _phases;

		float tx = 0, ty = 0;
		for (int i = 0; i < frequencies.Length; ++i) {
			var fx = deformationFrequency * frequencies [i].x;
			var fy = deformationFrequency * frequencies [i].y;
			tx += fx * Mathf.Cos (t * fx + phases [i].x);
			ty += fy * Mathf.Cos (t * fy + phases [i].y);
		}

		tx *= deformationScale.x / frequencies.Length;
		ty *= deformationScale.y / frequencies.Length;

		return new Vector3 (tx, ty, 1).normalized;
	}

	public TapePathPoint GetRayPoint (float t)
	{
		return new TapePathPoint (GetRayPosition (t), GetRayTangent (t));
	}

	private void Reload ()
	{
		var random = new System.Random (randomSeed);
		var frequencies = new Vector2[termsCount];
		for (int i = 0; i < frequencies.Length; ++i) {
			frequencies [i].x = (float)(random.NextDouble ());
			frequencies [i].y = (float)(random.NextDouble ());
		}
		_frequencies = frequencies;

		var phases = new Vector2[termsCount];
		for (int i = 0; i < phases.Length; ++i) {
			phases [i].x = (float)(random.NextDouble () * 2.0 * System.Math.PI);
			phases [i].y = (float)(random.NextDouble () * 2.0 * System.Math.PI);
		}
		_phases = phases;
	}

	public struct TapePathPoint {

		public Vector3 position;
		public Quaternion rotation;

		public TapePathPoint(Vector3 position, Vector3 tangent)
		{
			this.position = position;
			var binormal = Vector3.Cross(Vector3.up, tangent).normalized;
			var normal = Vector3.Cross(tangent, binormal);
			this.rotation = Quaternion.LookRotation(tangent, normal);
		}

		public Vector3 LocalToWorld(Vector3 point) {
			return position + rotation * point;
		}

		public Vector3 WorldToLocal(Vector3 point) {
			return Quaternion.Inverse(rotation) * (point - position);
		}

		public Vector3 LocalToWorldDirection(Vector3 direction) {
			return rotation * direction;
		}
	}

}