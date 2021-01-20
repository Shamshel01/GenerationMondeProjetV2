using System.Drawing;
using System.Runtime.InteropServices.ComTypes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

	public Vector2 regionSize = Vector2.one;
	public int num;
	public PoissonDiscSampling.Object[] objects;
	public bool autoUpdate;
    
    List<PoissonDiscSampling.PoissonCoord> points;

	void OnValidate() {
		points = PoissonDiscSampling.GeneratePoints(objects, regionSize, num);
	}

	void OnDrawGizmos() {
		Gizmos.DrawWireCube(regionSize/2, regionSize);
		
		if (!autoUpdate) {
			return;
		}

		if (points != null) {
            foreach (PoissonDiscSampling.PoissonCoord point in points) {
				Color color = objects[point.index].color;
				color.a  = 1.0f;
				Gizmos.color = color;
                Gizmos.DrawSphere(point.coords, objects[point.index].radius/2);
            }
		}
	}
}
