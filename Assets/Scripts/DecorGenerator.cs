using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DecorGenerator
{

	public struct PoissonCoord {
		public UnityEngine.Vector2 coords;
		public int index;
	}

    [System.Serializable]
    public struct Decor {
        public string name;
        public int number;
        public int seed;
        public float scale;
        public float scaleDif;
        public float radius;
		public int rejectionSamples;
        public GameObject mesh;
        public Color color;
    }

    public static List<UnityEngine.Vector2> GenerateDecor(int width, int height, int decors, int seed, bool[,] regionMap) {

        System.Random prng = new System.Random(seed);
		List<UnityEngine.Vector2> decorCoords = new List<UnityEngine.Vector2>();
        
		for (int i = 0; i < decors; i++) {
			int x = prng.Next(0, width);
			int y = prng.Next(0, height);

            if (regionMap[x, y]) {
			    decorCoords.Add(new UnityEngine.Vector2(x, y));
            }
		}

        return decorCoords;
    }

    public static Color[] GenerateDecorMap(Color[] decorMap, int width, int height, Decor[] decors, bool[,] regionMap, int numberOfPoints) {

		if (decorMap.Length != width*height) {
            // wrong width and height provided
            return decorMap;
        }

        UnityEngine.Vector2 sampleRegionSize = new UnityEngine.Vector2(width, height);

        for (int i = 0; i < decors.Length; i++) {
			List<PoissonCoord> decorCoords = GeneratePoints(decors, sampleRegionSize, numberOfPoints, regionMap);
            for (int j = 0; j < decorCoords.Count; j++) {
               	if ((int)(decorCoords[j].coords.y*width + decorCoords[j].coords.x) < decorMap.Length){
                   	decorMap[(int)(decorCoords[j].coords.y*width + decorCoords[j].coords.x)] = decors[i].color;
               	}
            }
		}

        return decorMap;
    }

	public static List<PoissonCoord> GeneratePoints(Decor[] objects, UnityEngine.Vector2 sampleRegionSize, int numberOfPoints, bool[,] regionMap) {

		List<PoissonCoord> points = new List<PoissonCoord>();
		List<PoissonCoord> spawnPoints = new List<PoissonCoord>();
		
		if (objects == null || objects.Length <= 0) {
			return points;
		} 

		System.Random rng = new System.Random();

		PoissonCoord startPoint;
		startPoint.coords = sampleRegionSize/2;

		startPoint.index = UnityEngine.Random.Range(0, objects.Length);

		spawnPoints.Add(startPoint);
		while (spawnPoints.Count > 0 && points.Count < numberOfPoints) {

			int objIndex = UnityEngine.Random.Range(0, objects.Length);
			float radius = objects[objIndex].radius;
			float rejectionSamples = objects[objIndex].rejectionSamples;
			

			int spawnIndex = UnityEngine.Random.Range(0,spawnPoints.Count);
			PoissonCoord spawnCentre = spawnPoints[spawnIndex];
			bool candidateAccepted = false;

			for (int i = 0; i < rejectionSamples; i++)
			{
				float randX = (float)rng.NextDouble()*sampleRegionSize.x;
				float randY = (float)rng.NextDouble()*sampleRegionSize.y;
			
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
		
		List<PoissonCoord> coords = new List<PoissonCoord>();
		for (int i = 0; i < points.Count; i++) {
			int x = (int)Mathf.Round(points[i].coords.x);
			int y =  (int)Mathf.Round(points[i].coords.y);
			if (x >= regionMap.GetLength(0) || y >= regionMap.GetLength(1)) {
				continue;
			}
			if (regionMap[x, y]) {
				coords.Add(points[i]);
			}
		}
		return coords;
	}

	static bool IsValid(PoissonCoord candidate, UnityEngine.Vector2 sampleRegionSize, List<PoissonCoord> points, Decor[] objects) {
		
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
}
