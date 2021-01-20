using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EndlessTerrain : MonoBehaviour {

	public const float maxViewDst = 1500;
	public Transform viewer;
	static MapGenerator mapGenerator;
	public static Vector2 viewerPosition;
	int chunkSize;
	int chunksVisibleInViewDst;
	public Material mapMaterial;

	TerrainType[] regions;

	Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

	void Start() {
		regions = FindObjectOfType<MapGenerator>().regions;
		mapGenerator = FindObjectOfType<MapGenerator>();
		chunkSize = MapGenerator.sizeMapChunk - 1;
		chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);
	}

	void Update() {
		viewerPosition = new Vector2 (viewer.position.x, viewer.position.z);
		UpdateVisibleChunks ();
	}
		
	void UpdateVisibleChunks() {

		for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++) {
			terrainChunksVisibleLastUpdate [i].SetVisible (false);
		}
		terrainChunksVisibleLastUpdate.Clear ();
			
		int currentChunkCoordX = Mathf.RoundToInt (viewerPosition.x / chunkSize);
		int currentChunkCoordY = Mathf.RoundToInt (viewerPosition.y / chunkSize);

		for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++) {
			for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++) {
				Vector2 viewedChunkCoord = new Vector2 (currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

				if (terrainChunkDictionary.ContainsKey (viewedChunkCoord)) {
					terrainChunkDictionary [viewedChunkCoord].UpdateTerrainChunk ();
					if (terrainChunkDictionary [viewedChunkCoord].IsVisible ()) {
						terrainChunksVisibleLastUpdate.Add (terrainChunkDictionary [viewedChunkCoord]);
					}
				} else {
					terrainChunkDictionary.Add (viewedChunkCoord, new TerrainChunk (viewedChunkCoord, chunkSize, transform, mapMaterial,regions));
				}

			}
		}
	}


	public class DecorsChunk {

	}

	public class TerrainChunk {

		GameObject meshObject;
		Vector2 position;
		Bounds bounds;
		MeshRenderer meshRenderer;
		MeshFilter meshFilter;

		//MeshFilter planeWater;


		public TerrainChunk(Vector2 coord, int size, Transform parent, Material material, TerrainType[]  regions) {
			position = coord * size;
			bounds = new Bounds(position,Vector2.one * size);
			Vector3 positionV3 = new Vector3(position.x,0,position.y);

			meshObject = new GameObject("Terrain Chunk");


			string decorsObjectName = "Decors";
			GameObject decorsObject = new GameObject();
			decorsObject.name = decorsObjectName;
			decorsObject.tag = decorsObjectName;
			decorsObject.transform.SetParent(meshObject.transform);
			//List<GameObject> listRegionGameObject = new List<GameObject>();
			List<List<GameObject>> listAllGameDecordObject = new List<List<GameObject>>();

			for (int i =0; i< regions.Length ; i++) {

				List<GameObject> listBasicGameObject = new List<GameObject>();
				GameObject regionObject = new GameObject();
				regionObject.name = regions[i].name;
				regionObject.transform.SetParent(decorsObject.transform);
				//listRegionGameObject.Add(regionObject);
				print(regions[i].densityOfDecors);
				for (int j=0 ; j<regions[i].densityOfDecors ; j++) {
					GameObject basic = new GameObject();
					basic.name = regions[i].name+ j.ToString();
					basic.transform.SetParent(regionObject.transform);
					listBasicGameObject.Add(basic);
				}
				listAllGameDecordObject.Add(listBasicGameObject);
			}
			//print("terrainChunk size list  " + listRegionGameObject.Count);
			meshRenderer = meshObject.AddComponent<MeshRenderer>();
			meshFilter = meshObject.AddComponent<MeshFilter>();
			meshObject.transform.position = positionV3;
			meshObject.transform.parent = parent;
			meshRenderer.material = material;
			SetVisible(false);
			mapGenerator.RequestMapData(position,onMapDataReceive,decorsObject,listAllGameDecordObject);

		}



		public void UpdateTerrainChunk() {
			float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance (viewerPosition));
			bool visible = viewerDstFromNearestEdge <= maxViewDst;
			SetVisible (visible);
		}

		public void SetVisible(bool visible) {
			meshObject.SetActive (visible);
		}

		public bool IsVisible() {
			return meshObject.activeSelf;
		}

		void onMapDataReceive (MapData mapData) {
			mapGenerator.RequestMeshData(mapData,onMeshDataReceive);
		}

		void onMeshDataReceive (MeshData meshData) {
			meshFilter.mesh = meshData.CreateMesh();
			//planeWater.mesh = meshData.CreateMesh();
		}
	}
}