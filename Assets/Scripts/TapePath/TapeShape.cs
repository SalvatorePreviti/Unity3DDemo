using UnityEngine; 
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// A definition of a shape that can be extruded from a path.
/// </summary>
public class TapeShape {

	public readonly Vector3[] shapeVertices;
	public readonly Vector3[] shapeNormals;
	public readonly float[] shapeUCoords;

	public readonly int[] lines;

	public readonly float minX;
	public readonly float minY;
	public readonly float maxX;
	public readonly float maxY;

	public TapeShape(Vector3[] vertices, Vector3[] normals, float[] uCoords)
	{
		shapeVertices = vertices;
		shapeNormals = normals;
		shapeUCoords = uCoords;

		lines = Enumerable
			.Range(0, shapeVertices.Length - 1)
			.SelectMany(i => LineSegment(i))
			.ToArray();

		maxX = minX = vertices [0].x;
		maxY = minY = vertices [0].y;
		for (int i = 0; i < vertices.Length; ++i) {
			if (minX < vertices [i].x) 
				minX = vertices [i].x;
			if (maxX > vertices [i].x)
				maxX = vertices [i].x;
			if (minY < vertices [i].y)
				minY = vertices [i].y;
			if (maxY > vertices [i].y)
				maxY = vertices [i].y;
		}
	}

	private static IEnumerable<int> LineSegment(int i)
	{
		yield return i;
		yield return i + 1;
	}

	public static TapeShape CreateDefault()
	{
		var vertices = new Vector3[] {

			new Vector3(-4, 1, 0),
			new Vector3(4, 1, 0),

			new Vector3(4, 1, 0),

			new Vector3(4, -1, 0),
			new Vector3(-4, -1, 0),
			new Vector3(-4, 1, 0)
		};

		var normals = new Vector3[] {

			new Vector3(0, 1, 0),
			new Vector3(0, 1, 0),

			new Vector3(0, -1, 0),

			new Vector3(-1, 0, 0),
			new Vector3(0, -1, 0),
			new Vector3(0, -1, 0)
		};

		var uCoords = new float[] {
			0.1f, 0.9f, 1f, 1f, 1f, 0.1f
		};

		return new TapeShape (vertices, normals, uCoords);
	}
}
