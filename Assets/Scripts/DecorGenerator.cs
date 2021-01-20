using System.Net;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DecorGenerator
{

    [System.Serializable]
    public struct Decor {
        public string name;
        public int number;
        public int seed;
        public float scale;
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

    public static Color[] GenerateDecorMap(Color[] decorMap, int width, int height, Decor[] decors, bool[,] regionMap) {

		if (decorMap.Length != width*height) {
            // wrong width and height provided
            return decorMap;
        }

        for (int i = 0; i < decors.Length; i++) {
			List<UnityEngine.Vector2> decorCoords = DecorGenerator.GenerateDecor(width, height, decors[i].number, decors[i].seed, regionMap);
            for (int j = 0; j < decorCoords.Count; j++) {
               decorMap[(int)(decorCoords[j].y*width + decorCoords[j].x)] = decors[i].color;
            }
		}

        return decorMap;
    }
}
