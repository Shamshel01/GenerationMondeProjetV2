using System.Numerics;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PoissonDiscSampling {

	public struct PoissonCoord {
		public UnityEngine.Vector2 coords;
		public int index;
	}

	[System.Serializable]
	public struct Object {
		public float radius;
		public float level;
		public int rejectionSamples;
		public Color color;
	}

	public static List<PoissonCoord> GeneratePoints(Object[] objects, UnityEngine.Vector2 sampleRegionSize, int numberOfPoints) {

		List<PoissonCoord> points = new List<PoissonCoord>();
		List<PoissonCoord> spawnPoints = new List<PoissonCoord>();
		
		if (objects == null || objects.Length <= 0) {
			return points;
		} 

		System.Random rng = new System.Random();

		PoissonCoord startPoint;
		startPoint.coords = sampleRegionSize/2;

		// startPoint.index = GetRandomObject(objects);
		startPoint.index = UnityEngine.Random.Range(0, objects.Length);

		spawnPoints.Add(startPoint);
		while (spawnPoints.Count > 0 && points.Count < numberOfPoints) {

			// int objIndex = GetRandomObject(objects);

			int objIndex = UnityEngine.Random.Range(0, objects.Length);
			float radius = objects[objIndex].radius;
			float rejectionSamples = objects[objIndex].rejectionSamples;
			

			int spawnIndex = UnityEngine.Random.Range(0,spawnPoints.Count);
			PoissonCoord spawnCentre = spawnPoints[spawnIndex];
			bool candidateAccepted = false;

			for (int i = 0; i < rejectionSamples; i++)
			{

				// float angle = UnityEngine.Random.value * Mathf.PI * 2;
				// UnityEngine.Vector2 dir = new UnityEngine.Vector2(Mathf.Sin(angle), Mathf.Cos(angle));				
				// PoissonCoord candidate;
				// candidate.coords = spawnCentre.coords + dir * UnityEngine.Random.Range(radius, 2*radius);
				// candidate.index = objIndex;

				float randX = (float)rng.NextDouble() * sampleRegionSize.x;
				float randY = (float)rng.NextDouble() * sampleRegionSize.y;
			
				PoissonCoord candidate;
				candidate.coords = new UnityEngine.Vector2(randX, randY);
				candidate.index = objIndex;

				if (IsValid(candidate, sampleRegionSize, points, objects)) {
					points.Add(candidate);
					spawnPoints.Add(candidate);
					candidateAccepted = true;
					break;
				}
			}
			if (!candidateAccepted) {
				spawnPoints.RemoveAt(spawnIndex);
			}

		}
		
		return points;
	}

	static bool IsValid(PoissonCoord candidate, UnityEngine.Vector2 sampleRegionSize, List<PoissonCoord> points, Object[] objects) {
		
		UnityEngine.Vector2 candidateCoords = candidate.coords;
		if (candidateCoords.x >=0 && candidateCoords.x < sampleRegionSize.x && candidateCoords.y >= 0 && candidateCoords.y < sampleRegionSize.y) {
			for (int i = 0; i < points.Count; i++) {
				float radius = objects[points[i].index].radius;
				if ((candidateCoords - points[i].coords).magnitude < radius) {
					return false;
				}
			}
			return true;
		}
		return false;
	}

	static int GetRandomObject(Object[] objects) {

		System.Random rng = new System.Random();
		float rand = (float)rng.NextDouble();
		for (int i = 0; i < objects.Length; i++) {
			if (rand < objects[i].level) {
				return i;
			}
		}

		return 0;
	}
}