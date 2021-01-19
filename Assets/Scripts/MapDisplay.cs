using System.Globalization;
using UnityEngine;
using System.Collections;

public class MapDisplay : MonoBehaviour {

	public Renderer textureRender;
	public MeshFilter meshFilter;
	public MeshRenderer meshRenderer; 

	public void DrawTexture(Texture2D texture) {
		textureRender.sharedMaterial.mainTexture = texture;
		textureRender.transform.localScale = new Vector3(texture.width, 1, texture.height);
	}

	public void DrawMesh(MeshData meshData, float heightMultiplier, TerrainType[] regions) {
		meshFilter.sharedMesh = meshData.CreateMesh();
		//textureRender.material.,
		Vector4 vec = new Vector4(0.60f,0,0,0);
		Material mat = meshRenderer.sharedMaterial;
		configureShaderFromRegions(mat, heightMultiplier, regions);
		
	}


	public void configureShaderFromRegions(Material mat,float heightMultiplier, TerrainType[] regions){
		/*
	  	Vector1_BF79393F SandHeight
		Vector1_8BD4EFA4 GrassHeight
		Vector1_BF79393F RockHeight
		Vector1_D1BFBE94 SnowHeight
		Vector1_8E34B0D2 Water Height
		???WATER HEIGHT is include in SAND HEIGHT???*/


		// mat.SetFloat("Vector1_8E34B0D2",0);
		// mat.SetFloat("Vector1_6C7E0CAA",regions[0].height*heightMultiplier);
		// mat.SetFloat("Vector1_8BD4EFA4",regions[1].height*heightMultiplier);
		// mat.SetFloat("Vector1_BF79393F",regions[2].height*heightMultiplier);
		// mat.SetFloat("Vector1_D1BFBE94",regions[3].height*heightMultiplier);
		

		/*
		UnityEngine.Debug.Log("Water Height  " +mat.GetFloat("Vector1_8E34B0D2"));
		UnityEngine.Debug.Log("Sand Height  " +mat.GetFloat("Vector1_BF79393F"));
		UnityEngine.Debug.Log("Grass Height  " +mat.GetFloat("Vector1_8BD4EFA4"));
		UnityEngine.Debug.Log("Rock Height  " +mat.GetFloat("Vector1_BF79393F"));
		UnityEngine.Debug.Log("Snow Height  " +mat.GetFloat("Vector1_D1BFBE94"));
*/
		/*
	  	Texture2D_4044189C Sand Diffuse Texture
		Texture2D_CF86792A Grass Diffuse Texture
		Texture2D_1CD61B64 Rock Diffuse Texture
		Texture2D_E7EFAA73 Snow Diffuse Texture
		Texture2D_89E80D00 Water Texture*/


		// mat.SetTexture("Texture2D_89E80D00", regions[0].textureDiffuse);
		// mat.SetTexture("Texture2D_4044189C", regions[1].textureDiffuse);
		// mat.SetTexture("Texture2D_CF86792A", regions[2].textureDiffuse);
		// mat.SetTexture("Texture2D_1CD61B64", regions[3].textureDiffuse);
		// mat.SetTexture("Texture2D_E7EFAA73", regions[4].textureDiffuse);

		/*
		Vector1_FFA2A584 Water Tile Factor
	  	Vector1_5C52168 Sand Tile Factor
		Vector1_5D935A7F Grass Tile Factor
		Vector1_434FCB7A Rock Tile Factor
		Vector1_FCC6C7C1 Snow Tile Factor*/



		// mat.SetFloat("Vector1_FFA2A584", regions[0].TilesTexture);
		// mat.SetFloat("Vector1_5C52168", regions[1].TilesTexture);
		// mat.SetFloat("Vector1_5D935A7F", regions[2].TilesTexture);
		// mat.SetFloat("Vector1_434FCB7A", regions[3].TilesTexture);
		// mat.SetFloat("Vector1_FCC6C7C1", regions[4].TilesTexture);


	}
}