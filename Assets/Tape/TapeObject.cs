using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[ExecuteInEditMode]
[RequireComponent(typeof(TapePath))]
[RequireComponent(typeof(TapePathShape))]
[RequireComponent(typeof(TapeChunks))]
[RequireComponent (typeof(MeshRenderer))]
public class TapeObject : CoreBehaviour
{
	public int tapeAddress;

	void Update() {

		var tapePath = this.GetComponent<TapePath> ();

		var cellDepth = tapePath.cellDepth;
		var zCenter = this.tapeAddress * cellDepth + cellDepth * 0.5f;

		var p = tapePath.GetRayPoint (zCenter);


		//var z = tapePathShape.getZFromAddress (tapeAddress);

		/*tapePath.GetRayPoint (tapeHead.transform.position);*/

		var tapeHead = GameObject.Find("TapeHeadObject");
		tapeHead.transform.position = p.position;
		tapeHead.transform.rotation = p.rotation;
	}
}
