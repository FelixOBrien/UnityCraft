using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    public GameObject player;
    public Material textureAtlas;
    public static int columnHeight = 16;
    public static int chunkSize = 16;
    public static int radius = 2;
    public static Dictionary<string, Chunk> chunks;

    public static string BuildChunkName(Vector3 v)
    {
        return (int)v.x + "_" + (int)v.y + "_" + (int)v.z;
    }
    IEnumerator BuildChunkColumn()
    {
        for(int i = 0; i < columnHeight; i++)
        {
            Vector3 chunkPosition = new Vector3(this.transform.position.x, i * chunkSize, this.transform.position.z);
                Chunk c = new Chunk(chunkPosition, textureAtlas);
                c.chunk.transform.parent = this.transform;
                chunks.Add(c.chunk.name, c);
        }

        foreach(KeyValuePair<string, Chunk> c in chunks)
        {
            c.Value.DrawChunk();
            yield return null;
        }
    }
    IEnumerator BuildWorld()
    {
        int posx = (int)Mathf.Floor(player.transform.position.x / chunkSize);
        int posz = (int)Mathf.Floor(player.transform.position.z / chunkSize);
        for (int z = -radius; z <= radius; z++)
        {
            for(int x = -radius; x <= radius; x++)
            {
                for(int y = 0; y < columnHeight; y++)
                {
                    Vector3 chunkPosition = new Vector3((x+posx)*chunkSize, y * chunkSize, (z+posz)*chunkSize);
                    Chunk c = new Chunk(chunkPosition, textureAtlas);
                    c.chunk.transform.parent = this.transform;
                    chunks.Add(c.chunk.name, c);
                }
            }

        }

        foreach (KeyValuePair<string, Chunk> c in chunks)
        {
            c.Value.DrawChunk();
            yield return null;
        }
        player.SetActive(true);
    }
    // Start is called before the first frame update
    void Start()
    {
        player.SetActive(false);
        chunks = new Dictionary<string, Chunk>();
        this.transform.position = Vector3.zero;
        this.transform.rotation = Quaternion.identity;
        StartCoroutine(BuildWorld());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
