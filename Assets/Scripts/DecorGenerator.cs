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

    public static UnityEngine.Vector2[] GenerateDecor(int width, int height, int decors, int seed, bool[,] regionMap) {

        System.Random prng = new System.Random(seed);
        
        int cpt = 0;
		UnityEngine.Vector2[] decorCoords = new UnityEngine.Vector2[decors];
        
		while (cpt < decors) {
			int x = prng.Next(0, width);
			int y = prng.Next(0, height);

            if (regionMap[x, y]) {
			    decorCoords[cpt] = new UnityEngine.Vector2(x, y);
                cpt++;
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
			UnityEngine.Vector2[] decorCoords = DecorGenerator.GenerateDecor(width, height, decors[i].number, decors[i].seed, regionMap);
            for (int j = 0; j < decorCoords.Length; j++) {
               decorMap[(int)(decorCoords[j].y*width + decorCoords[j].x)] = decors[i].color;
            }
		}

        return decorMap;
    }
}
