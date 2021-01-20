using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EndlessTerrain : MonoBehaviour {

	public const float maxViewDst = 500;
	public Transform viewer;
	static MapGenerator mapGenerator;
	public static Vector2 viewerPosition;
	int chunkSize;
	int chunksVisibleInViewDst;
	public Material mapMaterial;

	Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

	void Start() {
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
					terrainChunkDictionary.Add (viewedChunkCoord, new TerrainChunk (viewedChunkCoord, chunkSize, transform, mapMaterial));
				}

			}
		}
	}

	public class TerrainChunk {

		GameObject meshObject;
		Vector2 position;
		Bounds bounds;
		MeshRenderer meshRenderer;
		MeshFilter meshFilter;


		public TerrainChunk(Vector2 coord, int size, Transform parent, Material material) {
			position = coord * size;
			bounds = new Bounds(position,Vector2.one * size);
			Vector3 positionV3 = new Vector3(position.x,0,position.y);

			meshObject = new GameObject("Terrain Chunk");


			string decorsObjectName = "Decors";
			GameObject decorsObject = new GameObject();
			decorsObject.name = decorsObjectName;
			decorsObject.tag = decorsObjectName;
			decorsObject.transform.SetParent(meshObject.transform);



			meshRenderer = meshObject.AddComponent<MeshRenderer>();
			meshFilter = meshObject.AddComponent<MeshFilter>();
			meshObject.transform.position = positionV3;
			meshObject.transform.parent = parent;
			meshRenderer.material = material;
			SetVisible(false);
			mapGenerator.RequestMapData(position,onMapDataReceive,decorsObject);
			
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
		}
	}

	public class DecorsManagerChunk {
		float[] NormalizeNumberofDecors;
		public DecorGenerator.Decor[] Initaldecors;

		

	}
}