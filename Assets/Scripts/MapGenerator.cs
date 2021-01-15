﻿using System.Text;
using System.Net;
using System.Drawing;
using System.Numerics;
using System;
using System.ComponentModel.DataAnnotations;
using UnityEngine;
using System.Collections;

public class MapGenerator : MonoBehaviour {


	// TO DO AND RANGE VALUES for variables 

	public enum DrawMode{NoiseMap, ColorMap, Mesh, DecorMap};
	public DrawMode drawMode;

	public float noiseScale;

	[Range(0,6)]
	public int levelOfDetail;


	[Range(0,50)]
	public int octaves;

	[Range(0,1)]
	public float persistance;
	public float lacunarity;
	public const int sizeMapChunk = 241;
	public int seed;
	public UnityEngine.Vector2 offset;



	public float heightMultiplier;
	public AnimationCurve meshHeightCurve;

	public bool autoUpdate;
	public TerrainType[] regions;


	public void GenerateMap() {
		float[,] noiseMap = Noise.GenerateNoiseMap(sizeMapChunk, sizeMapChunk, seed, noiseScale, octaves, persistance, lacunarity, offset);

		Color[] colorMap = new Color[sizeMapChunk*sizeMapChunk];
		for (int y = 0; y < sizeMapChunk; y++) {
			for (int x = 0; x < sizeMapChunk; x++) {
				float currentHeight = noiseMap[x, y];
				for (int i = 0; i < regions.Length; i++) {
					if (currentHeight <= regions[i].height) {
						colorMap[y*sizeMapChunk + x] = regions[i].colour;
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
			display.DrawTexture(TextureGenerator.ColorMapToTexture(colorMap, sizeMapChunk, sizeMapChunk));
		} else if (drawMode == DrawMode.Mesh) {
			display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, heightMultiplier, meshHeightCurve, levelOfDetail), heightMultiplier, regions);
			
			for (int i = 0; i < regions.Length; i++) {

				GameObject regionsObject = new GameObject();
				regionsObject.name = regions[i].name;
				regionsObject.transform.SetParent(decorsObject.transform);

				DecorGenerator.Decor[] decors = regions[i].decors;
				float low = 0;
				if (i != 0) {
					low = regions[i - 1].height;
				}
				bool[,] regionMap = GetRegion(sizeMapChunk, sizeMapChunk, noiseMap, regions[i].height, low);

				for (int j = 0; j < decors.Length; j++) {
					GameObject parentObject = new GameObject();
					parentObject.transform.SetParent(regionsObject.transform);
					parentObject.name = decors[j].name;
					UnityEngine.Vector2[] decorCoords = DecorGenerator.GenerateDecor(sizeMapChunk, sizeMapChunk, decors[j].number, decors[j].seed, regionMap);
					PlaceDecor(decorCoords, noiseMap, decors[j].name, decors[j].scale, decors[j].mesh, parentObject);
				}
			}

		} else if (drawMode == DrawMode.DecorMap) {
			Color[] decorMap = new Color[sizeMapChunk*sizeMapChunk];
			for (int i = 0; i < regions.Length; i++) {
				float low = 0;
				if (i != 0) {
					low = regions[i - 1].height;
				}
				bool[,] regionMap = GetRegion(sizeMapChunk, sizeMapChunk, noiseMap, regions[i].height, low);
				decorMap = DecorGenerator.GenerateDecorMap(colorMap, sizeMapChunk, sizeMapChunk, regions[i].decors, regionMap);
			}
			display.DrawTexture(TextureGenerator.ColorMapToTexture(decorMap, sizeMapChunk, sizeMapChunk));
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

	public void PlaceDecor(UnityEngine.Vector2[] decorCoords, float[,] heightMap, string name, float scale, GameObject decorObject, GameObject parentObject)
    {

		if (decorObject == null) {
			return;
		}
		
		int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width - 1)/-2f;
        float topLeftZ = (height - 1)/2f;

        for (int i = 0; i < decorCoords.Length; i++)
        {
			int x = (int)decorCoords[i].x;
			int y = (int)decorCoords[i].y;
			float currentHeight = heightMap[x, y]*heightMultiplier*meshHeightCurve.Evaluate(heightMap[x, y]);
            UnityEngine.Vector3 position = new UnityEngine.Vector3(topLeftX + x, currentHeight, topLeftZ - y);
            GameObject decor = Instantiate(decorObject, position, decorObject.transform.rotation);
			decor.name = name + "_" + i.ToString();
			decor.transform.localScale = new UnityEngine.Vector3(scale, scale, scale);
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

	[Range(0,1)]
	public float height;
	public DecorGenerator.Decor[] decors;
	public Color colour;

	public float TilesTexture;
	public Texture2D textureDiffuse;
	public Texture2D textureNormal;
}
