using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    public const float maxViewDistance = 400;
    public Transform viewer;
    public static Vector2 viewerPosition;
    int chunkSize;
    int chunksVisibleInViewDistance;
    // Start is called before the first frame update


    Dictionary<Vector2,TerrainChunk> terrainChunkDico = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainVisibleInTheLastUpdate;
    void Start()
    {
        chunkSize = MapGenerator.sizeMapChunk - 1; // If not => some problems
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance/chunkSize);
    }


    void UpdateVisibleChunks() {

        for (int i=0 ; i < terrainVisibleInTheLastUpdate.Count; i++) {
            terrainVisibleInTheLastUpdate[i].setVisible(false);
        }
        terrainVisibleInTheLastUpdate.Clear();



        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = - chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++) {
            for (int xOffset = - chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++) {
                Vector2 viewedChunkCoord = new Vector2 (currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if (terrainChunkDico.ContainsKey(viewedChunkCoord)) {
                    
                    terrainChunkDico[viewedChunkCoord].UpdateChunk();
                    if (terrainChunkDico[viewedChunkCoord].isVisible()) {
                        terrainVisibleInTheLastUpdate.Add(terrainChunkDico[viewedChunkCoord]);
                    }


                } else 
                    terrainChunkDico.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord,chunkSize));

            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
    }




    public class TerrainChunk {
        Vector2 position;
        GameObject meshObject;

        Bounds bounds;
        public TerrainChunk(Vector2 coord, int size) {
            position = coord * size;
            bounds = new Bounds(position,Vector2.one*size);
            Vector3 position3 = new Vector3(position.x,0,position.y);
            meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject.transform.position = position3;
            meshObject.transform.localScale = Vector3.one*size/10f;
            setVisible(false);
        }

        public void UpdateChunk() {

            float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;
            setVisible(false);
        }

        public void setVisible(bool visible) {
            meshObject.SetActive(visible);
        }

        public bool isVisible() {
            return meshObject.activeSelf;
        }
    }
}

