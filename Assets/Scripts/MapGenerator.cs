﻿using System.Security.Cryptography;
using System.Text;
using System.Net;
using System.Drawing;
using System.Numerics;
using System;
using System.ComponentModel.DataAnnotations;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

public class MapGenerator : MonoBehaviour {


	// TO DO AND RANGE VALUES for variables 

	public enum DrawMode{NoiseMap, ColorMap, Mesh, DecorMap}; 
	public DrawMode drawMode;

	public float noiseScale;
	public Noise.NormalizeMode normalizeMode;

	[Range(0,6)]
	public int levelOfDetail;


	[Range(0,8)]
	public int octaves;

	[Range(0,1)]
	public float persistance;
	public float lacunarity;
	public const int sizeMapChunk = 241;
	public int seed;

	
	public UnityEngine.Vector2 offset;

	public float heightMultiplier;
	public AnimationCurve meshHeightCurve;

	//public bool autoUpdate;
	public bool autoUpdate = false;
	public TerrainType[] regions;
	public Material waterMat;

	public float WaterLevel;
	Queue <MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
	Queue <MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

	
	public void drawMapEditor() {
		string decorsObjectName = "Decors";
		GameObject[] existingDecorObjects = GameObject.FindGameObjectsWithTag(decorsObjectName);
		for (int i = 0; i < existingDecorObjects.Length; i++) {
			DestroyImmediate(existingDecorObjects[i]);
		}

		GameObject decorsObject = new GameObject();
		decorsObject.name = decorsObjectName;
		decorsObject.tag = decorsObjectName;
		MapData mapData = GenerateChunkMap(UnityEngine.Vector2.zero);

		MapDisplay display = FindObjectOfType<MapDisplay> ();
		if (drawMode == DrawMode.NoiseMap) {
			display.DrawTexture(TextureGenerator.HeightMapToTexture(mapData.heightMap));
		} else if (drawMode == DrawMode.ColorMap) {
			display.DrawTexture(TextureGenerator.ColorMapToTexture(mapData.colourMap, sizeMapChunk, sizeMapChunk));
		} else if (drawMode == DrawMode.Mesh) {
			display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, heightMultiplier, meshHeightCurve, levelOfDetail), heightMultiplier, regions);
			//display.DrawMeshPlane(MeshGenerator.generatePlaneMesh(mapData.heightMap), waterMat);
			AffectDecorsMainThread(decorsObject, mapData, UnityEngine.Vector2.zero);
			CreateWaterMesh(decorsObject, mapData, WaterLevel, UnityEngine.Vector2.zero,display);
		} 
	}


	public void CreateWaterMesh(GameObject decorsObject, MapData mapData, float WaterLevel, UnityEngine.Vector2 offSetPosition, MapDisplay display){
		GameObject WaterObject = new GameObject();
		WaterObject.transform.name = "WaterMesh";
		WaterObject.transform.SetParent(decorsObject.transform);
		MeshFilter meshFilter = WaterObject.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = WaterObject.AddComponent<MeshRenderer>();
		meshFilter.sharedMesh = display.DrawMeshPlane(MeshGenerator.GeneratePlaneWaterMesh(mapData,WaterLevel));
		meshRenderer.material = waterMat;

		WaterObject.transform.position = new UnityEngine.Vector3(WaterObject.transform.position.x + offSetPosition.x,WaterLevel,WaterObject.transform.position.z + offSetPosition.y);

		//display.DrawMeshPlane()

	}
	public void AffectDecorsMainThread(GameObject decorsObject, MapData mapData, UnityEngine.Vector2 offSetChunk) {
			UnityEngine.Vector2 sampleRegionSize = new UnityEngine.Vector2(sizeMapChunk, sizeMapChunk);
			for (int i = 0; i < regions.Length; i++) {
				
				GameObject regionsObject = new GameObject();
				regionsObject.name = regions[i].name;
				regionsObject.transform.SetParent(decorsObject.transform);

				DecorGenerator.Decor[] decors = regions[i].decors;
				
				float low = regions[i].height;
				float high = 1;
				if (i != regions.Length - 1) {
					high = regions[i + 1].height;
				}
				bool[,] regionMap = GetRegion(sizeMapChunk, sizeMapChunk, mapData.heightMap, low, high);
				List<DecorGenerator.PoissonCoord> decorCoords = DecorGenerator.GeneratePoints(decors, sampleRegionSize, regions[i].densityOfDecors, regionMap);
				PlaceDecor(decorCoords, mapData.heightMap, decors, regionsObject, offSetChunk);		
				

			}

	}

	public void RequestMapData(UnityEngine.Vector2 offSetCoord, Action<MapData> callback) {
		ThreadStart threadStart = delegate {
			mapDataThread(offSetCoord, callback);
		};
		new Thread(threadStart).Start();
	}

	void mapDataThread(UnityEngine.Vector2 offsetCoord,Action <MapData> callback){

		MapData mapData = GenerateChunkMap(offsetCoord);
		lock (mapDataThreadInfoQueue) {
			mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback,mapData));
		}
	}

	
	public void RequestMeshData(MapData mapData, Action<MeshData> callback) {
		ThreadStart threadStart = delegate {
			MeshDataThread (mapData, callback);
		};

		new Thread (threadStart).Start ();
	}

	void MeshDataThread(MapData mapData, Action <MeshData> callback){
		MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap,heightMultiplier,meshHeightCurve,levelOfDetail);
		lock (meshDataThreadInfoQueue) {
			meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback,meshData));
		}
	}

	void Update()
	{
		if (mapDataThreadInfoQueue.Count > 0) {
			for (int i =0; i< mapDataThreadInfoQueue.Count; i++) {
				MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
				threadInfo.callback(threadInfo.parameter);
			}
		}
		if (meshDataThreadInfoQueue.Count > 0) {
			for (int i =0; i< meshDataThreadInfoQueue.Count; i++) {
				MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
				threadInfo.callback(threadInfo.parameter);
			}
		}
	}

	MapData GenerateChunkMap(UnityEngine.Vector2 offsetCoord) {
		float[,] noiseMap = Noise.GenerateNoiseMap(sizeMapChunk, sizeMapChunk, seed, noiseScale, octaves, persistance, lacunarity,offsetCoord + offset, normalizeMode);

		Color[] colorMap = new Color[sizeMapChunk*sizeMapChunk];
		for (int y = 0; y < sizeMapChunk; y++) {
			for (int x = 0; x < sizeMapChunk; x++) {
				float currentHeight = noiseMap[x, y];

				for (int i = 0; i < regions.Length-1; i++) {
					if (currentHeight < regions[i+1].height) {
						colorMap[y*sizeMapChunk + x] = regions[i].colour;
						break;
					}
				}						
			}
		}
		return new MapData(noiseMap,colorMap);
	}

	public bool[,] GetRegion(int width, int height, float[,] heightMap, float low, float high) {
		bool[,] region = new bool[width, height];
		
		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				float currentHeight = heightMap[i, j];
				region[i, j] = currentHeight > low && currentHeight < high;
			}
		}
		return region;
	}

	public void PlaceDecor(List<DecorGenerator.PoissonCoord> decorCoords, float[,] heightMap, DecorGenerator.Decor[] decors, GameObject parentObject, UnityEngine.Vector2 offSetChunk) {
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
            UnityEngine.Vector3 position = new UnityEngine.Vector3(topLeftX + x +offSetChunk.x, currentHeight, topLeftZ - y + offSetChunk.y);
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


	struct MapThreadInfo<T> {
		public readonly Action<T> callback;
		public readonly T parameter;

		public MapThreadInfo(Action<T> callback , T paramater) {
			this.callback = callback;
			this.parameter = paramater;
		}

	}
}

[System.Serializable]
public struct TerrainType {
	public string name;
	public float height;
	public int densityOfDecors;
	public DecorGenerator.Decor[] decors;
	public Color colour;

	public float TilesTexture;
	public Texture2D textureDiffuse;
	public Texture2D textureNormal;
}

public struct MapData {
	public float[,] heightMap;
	public Color[] colourMap;
	public MapData(float[,] heightMap, Color[] colourMap) {
		this.heightMap = heightMap;
		this.colourMap = colourMap;
	}
}
