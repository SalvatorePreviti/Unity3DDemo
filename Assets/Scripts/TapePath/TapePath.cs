using UnityEngine; 
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TapePath
{
	const int TermsCount = 5;

	public readonly int Seed;

	public readonly Vector3 Scale;

	private Vector2[] _frequencies = new Vector2[TermsCount];
	private Vector2[] _phases = new Vector2[TermsCount];

	public TapePath(Vector3 scale, double frequencyMultiplier, int seed)
	{
		Scale = scale;

		var random = new System.Random(seed);
		for (int i = 0; i < TermsCount; ++i)
		{
			_frequencies[i] = new Vector2(
				(float)(random.NextDouble() * frequencyMultiplier),
				(float)(random.NextDouble() * frequencyMultiplier));
			_phases[i] = new Vector2(
				(float)(random.NextDouble() * 2.0 * System.Math.PI),
				(float)(random.NextDouble() * 2.0 * System.Math.PI));
		}
	}

	public Vector3 GetPosition(float t)
	{
		var frequencies = _frequencies;
		var phases = _phases;

		float px = 0, py = 0;
		for (int i = 0; i < TermsCount; ++i) 
		{
			px += Mathf.Sin(t * frequencies[i].x + phases[i].x);
			py += Mathf.Sin(t * frequencies[i].y + phases[i].y);
		}

		px *= Scale.x / TermsCount;
		py *= Scale.y / TermsCount;

		return new Vector3(px, py, t * Scale.z);
	}

	public Vector3 GetTangent(float t)
	{
		var frequencies = _frequencies;
		var phases = _phases;

		float tx = 0, ty = 0;
		for (int i = 0; i < TermsCount; ++i) 
		{
			tx += frequencies[i].x * Mathf.Cos(t * frequencies[i].x + phases[i].x);
			ty += frequencies[i].y * Mathf.Cos(t * frequencies[i].y + phases[i].y);
		}

		tx *= Scale.x / TermsCount;
		ty *= Scale.y / TermsCount;

		return new Vector3(tx, ty, Scale.z).normalized;
	}

	public TapePathPoint GetPoint(float t)
	{
		return new TapePathPoint (GetPosition (t), GetTangent (t));
	}
}