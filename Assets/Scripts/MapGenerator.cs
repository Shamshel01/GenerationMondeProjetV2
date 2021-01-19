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
		MapData mapData = GenerateChunkMap(UnityEngine.Vector2.zero, decorsObject);

		MapDisplay display = FindObjectOfType<MapDisplay> ();
		if (drawMode == DrawMode.NoiseMap) {
			display.DrawTexture(TextureGenerator.HeightMapToTexture(mapData.heightMap));
		} else if (drawMode == DrawMode.ColorMap) {
			display.DrawTexture(TextureGenerator.ColorMapToTexture(mapData.colourMap, sizeMapChunk, sizeMapChunk));
		} else if (drawMode == DrawMode.Mesh) {
			display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, heightMultiplier, meshHeightCurve, levelOfDetail), heightMultiplier, regions);


			for (int i = 0; i < regions.Length; i++) {
				
			GameObject regionsObject = new GameObject();
			regionsObject.name = regions[i].name;
			regionsObject.transform.SetParent(decorsObject.transform);

			DecorGenerator.Decor[] decors = regions[i].decors;
			float high = 1;
			if (i != regions.Length -1 ) {
				high = regions[i+1].height;
			} 
			bool[,] regionMap = GetRegion(sizeMapChunk, sizeMapChunk, mapData.heightMap, regions[i].height, high);
		
			
			for (int j = 0; j < decors.Length; j++) {
				GameObject parentObject = new GameObject();
				parentObject.transform.SetParent(regionsObject.transform);
				parentObject.name = decors[j].name;

			//	UnityEngine.Vector2[] decorCoords = DecorGenerator.GenerateDecor(sizeMapChunk, sizeMapChunk, decors[j].number, decors[j].seed, regionMap);
				
				System.Random prng = new System.Random(seed);
				print(decors.Length);
				int cpt = 0;
				int maxLoop = 3000;
				int lp = 0;
				UnityEngine.Vector2[] decorCoords = new UnityEngine.Vector2[decors[j].number];
				print(decors[j].number);
				while (cpt < decors[j].number && lp < maxLoop) {
					
					int x = prng.Next(0, sizeMapChunk);
					int y = prng.Next(0, sizeMapChunk);
					if (regionMap[x, y]) {
						decorCoords[cpt] = new UnityEngine.Vector2(x, y);
						cpt++;
					}
					lp++;
				}

				PlaceDecor(decorCoords, mapData.heightMap, decors[j].name, decors[j].scale, decors[j].mesh, parentObject);
			}
		}
			
		
		/*} else if (drawMode == DrawMode.DecorMap) {
			Color[] decorMap = new Color[sizeMapChunk*sizeMapChunk];
			for (int i = 0; i < regions.Length; i++) {
				float low = 0;
				if (i != 0) {
					low = regions[i - 1].height;
				}
				bool[,] regionMap = GetRegion(sizeMapChunk, sizeMapChunk, mapData.heightMap, regions[i].height, low);
				decorMap = DecorGenerator.GenerateDecorMap(mapData.colourMap, sizeMapChunk, sizeMapChunk, regions[i].decors, regionMap);
			}
			display.DrawTexture(TextureGenerator.ColorMapToTexture(decorMap, sizeMapChunk, sizeMapChunk));*/
		}
	}


	public void RequestMapData(UnityEngine.Vector2 offSetCoord, Action<MapData> callback, GameObject decorsThreadSafe) {
		ThreadStart threadStart = delegate {
			mapDataThread(offSetCoord, callback, decorsThreadSafe);
		};
		new Thread(threadStart).Start();
	}

	void mapDataThread(UnityEngine.Vector2 offsetCoord,Action <MapData> callback, GameObject decorsThreadSafe){

		MapData mapData = GenerateChunkMap(offsetCoord, decorsThreadSafe);
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

	MapData GenerateChunkMap(UnityEngine.Vector2 offsetCoord, GameObject decorsThreadSafe) {
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
		return new MapData(noiseMap,colorMap,decorsThreadSafe);
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
			UnityEngine.Debug.Log("Decors null");
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
			//UnityEngine.Debug.Log("x : "+x.ToString()+ " y: " +y.ToString());
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
	public DecorGenerator.Decor[] decors;
	public Color colour;

	public float TilesTexture;
	public Texture2D textureDiffuse;
	public Texture2D textureNormal;
}

public struct MapData {
	public float[,] heightMap;
	public Color[] colourMap;
	public GameObject DecorChunckObjects;
	public MapData(float[,] heightMap, Color[] colourMap, GameObject arrayDecorsObject) {
		this.heightMap = heightMap;
		this.colourMap = colourMap;
		this.DecorChunckObjects = arrayDecorsObject;
	}
}
