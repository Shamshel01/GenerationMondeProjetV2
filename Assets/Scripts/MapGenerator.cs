using System.ComponentModel.DataAnnotations;
using Microsoft.Win32;
using UnityEngine;
using System.Collections;

public class MapGenerator : MonoBehaviour {

	public enum DrawMode{NoiseMap, ColorMap, Mesh};
	public DrawMode drawMode;

	public int mapWidth;
	public int mapHeight;
	public float noiseScale;

	public int octaves;
	public float persistance;
	public float lacunarity;

	public int seed;
	public Vector2 offset;

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

		MapDisplay display = FindObjectOfType<MapDisplay> ();
		if (drawMode == DrawMode.NoiseMap) {
			display.DrawTexture(TextureGenerator.HeightMapToTexture(noiseMap));
		} else if (drawMode == DrawMode.ColorMap) {
			display.DrawTexture(TextureGenerator.ColorMapToTexture(colorMap, mapWidth, mapHeight));
		} else if (drawMode == DrawMode.Mesh) {
			display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, heightMultiplier, meshHeightCurve), TextureGenerator.ColorMapToTexture(colorMap, mapWidth, mapHeight));
		}
	}
	
}

[System.Serializable]
public struct TerrainType {
	public string name;
	public float height;
	public Color colour;
}