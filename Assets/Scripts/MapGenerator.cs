using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Drawing;
using System.Numerics;
using System;
using System.ComponentModel.DataAnnotations;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour {

	public enum DrawMode{NoiseMap, ColorMap, Mesh, DecorMap};
	public DrawMode drawMode;

	public int mapWidth;
	public int mapHeight;
	public float noiseScale;

	public int octaves;
	public float persistance;
	public float lacunarity;

	public int seed;
	public UnityEngine.Vector2 offset;

	public float heightMultiplier;
	public AnimationCurve meshHeightCurve;

	public bool autoUpdate;
	public TerrainType[] regions;


	public void GenerateMap() {
		float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistance, lacunarity, offset);

		Color[] colorMap = new Color[mapWidth*mapHeight];
		for (int y = 0; y < mapHeight; y++) {
			for (int x = 0; x < mapWidth; x++) {
				float currentHeight = noiseMap[x, y];
				for (int i = 0; i < regions.Length; i++) {
					if (currentHeight <= regions[i].height) {
						colorMap[y*mapWidth + x] = regions[i].colour;
						break;
					}
				}
			}
		}

		string decorsObjectName = "Decors";
		GameObject[] existingDecorObjects = GameObject.FindGameObjectsWithTag(decorsObjectName);
		for (int i = 0; i < existingDecorObjects.Length; i++) {
			DestroyImmediate(existingDecorObjects[i]);
		}

		GameObject decorsObject = new GameObject();
		decorsObject.name = decorsObjectName;
		decorsObject.tag = decorsObjectName;

		MapDisplay display = FindObjectOfType<MapDisplay> ();
		if (drawMode == DrawMode.NoiseMap) {
			display.DrawTexture(TextureGenerator.HeightMapToTexture(noiseMap));
		} else if (drawMode == DrawMode.ColorMap) {
			display.DrawTexture(TextureGenerator.ColorMapToTexture(colorMap, mapWidth, mapHeight));
		} else if (drawMode == DrawMode.Mesh) {
			UnityEngine.Vector2 sampleRegionSize = new UnityEngine.Vector2(mapWidth, mapHeight);
			display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, heightMultiplier, meshHeightCurve), heightMultiplier, regions);
			
			for (int i = 0; i < regions.Length; i++) {

				GameObject regionsObject = new GameObject();
				regionsObject.name = regions[i].name;
				regionsObject.transform.SetParent(decorsObject.transform);

				DecorGenerator.Decor[] decors = regions[i].decors;
				float low = 0;
				if (i != 0) {
					low = regions[i - 1].height;
				}
				bool[,] regionMap = GetRegion(mapWidth, mapHeight, noiseMap, regions[i].height, low);

				List<DecorGenerator.PoissonCoord> decorCoords = DecorGenerator.GeneratePoints(decors, sampleRegionSize, regions[i].numberOfDecors, regionMap);
				PlaceDecor(decorCoords, noiseMap, decors, regionsObject);
			}

		} else if (drawMode == DrawMode.DecorMap) {
			Color[] decorMap = new Color[mapWidth*mapHeight];
			for (int i = 0; i < regions.Length; i++) {
				float low = 0;
				if (i != 0) {
					low = regions[i - 1].height;
				}
				bool[,] regionMap = GetRegion(mapWidth, mapHeight, noiseMap, regions[i].height, low);
				decorMap = DecorGenerator.GenerateDecorMap(colorMap, mapWidth, mapHeight, regions[i].decors, regionMap, regions[i].numberOfDecors);
			}
			display.DrawTexture(TextureGenerator.ColorMapToTexture(decorMap, mapWidth, mapHeight));
		}
	}

	public bool[,] GetRegion(int width, int height, float[,] heightMap, float high, float low) {
		bool[,] region = new bool[width, height];

		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				float currentHeight = heightMap[i, j];
				region[i, j] = currentHeight > low && currentHeight < high;
			}
		}

		return region;
	}

	public void PlaceDecor(List<DecorGenerator.PoissonCoord> decorCoords, float[,] heightMap, DecorGenerator.Decor[] decors, GameObject parentObject) {
		int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width - 1)/-2f;
        float topLeftZ = (height - 1)/2f;
 
        System.Random prng = new System.Random();

        for (int i = 0; i < decorCoords.Count; i++)
        {
			int x = (int)decorCoords[i].coords.x;
			int y = (int)decorCoords[i].coords.y;

			float currentHeight = heightMap[x, y]*heightMultiplier*meshHeightCurve.Evaluate(heightMap[x, y]);
			DecorGenerator.Decor decorToPlace = decors[decorCoords[i].index];
            UnityEngine.Vector3 position = new UnityEngine.Vector3(topLeftX + x, currentHeight, topLeftZ - y);
            GameObject decor = Instantiate(decorToPlace.mesh, position, decorToPlace.mesh.transform.rotation);
			decor.name = name + "_" + i.ToString();

			float scaleDif = decorToPlace.scaleDif;
			if (scaleDif > 1) {
				scaleDif = 1;
			} else if (scaleDif < 0) {
				scaleDif = 0;
			}

			float randomScale = decorToPlace.scale*(1.0f + (2*(float)prng.NextDouble() - 1)*scaleDif);
			decor.transform.localScale = new UnityEngine.Vector3(randomScale, randomScale, randomScale);
            UnityEngine.Quaternion rotation = UnityEngine.Random.rotation;
            rotation.x = decor.transform.rotation.x;
            rotation.z = decor.transform.rotation.z;
            decor.transform.rotation = rotation;
            decor.transform.SetParent(parentObject.transform);
        }
    }
}

[System.Serializable]
public struct TerrainType {
	public string name;
	public float height;
	public int numberOfDecors;
	public DecorGenerator.Decor[] decors;
	public Color colour;
	public float TilesTexture;
	public Texture2D textureDiffuse;
	public Texture2D textureNormal;
}
