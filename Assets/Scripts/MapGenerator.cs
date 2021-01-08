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

		int tempMapWidth = mapWidth *20;
		int tempMapHeight = mapHeight *20;
		Color[] colorMap = new Color[tempMapWidth*tempMapHeight];
		for (int y = 0; y < tempMapHeight; y++) {
			for (int x = 0; x < tempMapWidth; x++) {
				float currentHeight = noiseMap[(int)(x/20),(int)(y/20)];
				
				float dx = (float)x/20.0f - (int)(x/20);
				float dy = (float)y/20.0f - (int)(y/20);
				/*Vector2 vec = new Vector2(dx,dy);
				if ((vec.magnitude < 0.5f)){
					currentHeight = 0;

				}*/
				for (int i = 0; i < regions.Length; i++) {
					if (currentHeight <= regions[i].height) {
						colorMap[y*tempMapWidth + x] = regions[i].colour;
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
			display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, heightMultiplier, meshHeightCurve), TextureGenerator.ColorMapToTexture(colorMap, tempMapWidth, tempMapHeight));
		}
	}
	
}

[System.Serializable]
public struct TerrainType {
	public string name;
	public float height;
	public Color colour;
}