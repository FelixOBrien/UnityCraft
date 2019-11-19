using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Realtime.Messaging.Internal;

public class World : MonoBehaviour
{
    public GameObject player;
    public Material textureAtlas;
    public static int columnHeight = 16;
    public static int chunkSize = 16;
    public static int radius = 7;
    public static ConcurrentDictionary<string, Chunk> chunks;
    public Slider loadingAmount;
    public Camera cam;
    public Button playButton;
    bool firstbuild = true;
    bool firstBuildDraw = false;
    bool building = false;
    public Vector3 lastBuildPos;

    CoroutineQueue queue;
    public static uint maxCoroutines = 1000;

    public static string BuildChunkName(Vector3 v)
    {
        return (int)v.x + "_" + (int)v.y + "_" + (int)v.z;
    }

    void BuildChunkAt(int x, int y, int z)
    {
        Vector3 chunkPosition = new Vector3(x * chunkSize, y * chunkSize, z * chunkSize);
        string n = BuildChunkName(chunkPosition);
        Chunk c;
        if(!chunks.TryGetValue(n, out c))
        {
            c = new Chunk(chunkPosition, textureAtlas);
            c.chunk.transform.parent = this.transform;
            chunks.TryAdd(c.chunk.name, c);
        }

    }
    //Flood Fill Algorithm
    IEnumerator BuildRecursiveWorld(int x, int y, int z, int rad)
    {
        if (rad <= 0) yield break;
        rad--;
        //Front
        BuildChunkAt(x, y, z + 1);
        queue.Run(BuildRecursiveWorld(x, y, z + 1, rad));
        yield return null;

        //Back
        BuildChunkAt(x, y, z - 1);
        queue.Run(BuildRecursiveWorld(x, y, z - 1, rad));
        yield return null;

        //Left
        BuildChunkAt(x-1, y, z);
        queue.Run(BuildRecursiveWorld(x-1, y, z , rad));
        yield return null;

        //Right
        BuildChunkAt(x + 1, y, z);
        queue.Run(BuildRecursiveWorld(x + 1, y, z, rad));
        yield return null;

        //Up
        BuildChunkAt(x, y + 1, z);
        queue.Run(BuildRecursiveWorld(x, y + 1, z, rad));
        yield return null;

        //Down
        BuildChunkAt(x, y - 1, z);
        queue.Run(BuildRecursiveWorld(x, y - 1, z, rad));
        yield return null;

        //Creation of chunks has been completed, wait for drawing.
        if (firstbuild) firstBuildDraw = true;
    }

    public void BuildNearPlayer()
    {
        StopCoroutine("BuildRecursiveWorld");
        queue.Run(BuildRecursiveWorld((int)player.transform.position.x / chunkSize, (int)player.transform.position.y / chunkSize, (int)player.transform.position.z / chunkSize, radius));
    }

    //DRAWS ALL CHUNKS THAT AREN'T DRAWN YET
    IEnumerator DrawChunks()
    {
        foreach(KeyValuePair<string, Chunk> c in chunks)
        {
            if(c.Value.status == Chunk.ChunkStatus.DRAW)
            {
                c.Value.DrawChunk();
                c.Value.status = Chunk.ChunkStatus.DONE;
            }

            yield return null;
            //Drawing has finished and therefore is ready to play
            if (firstBuildDraw) preStart();

        }
    }
    //NOT USED ANYMORE
    IEnumerator BuildChunkColumn()
    {
        for(int i = 0; i < columnHeight; i++)
        {
            Vector3 chunkPosition = new Vector3(this.transform.position.x, i * chunkSize, this.transform.position.z);
                Chunk c = new Chunk(chunkPosition, textureAtlas);
                c.chunk.transform.parent = this.transform;
                chunks.TryAdd(c.chunk.name, c);
        }

        foreach(KeyValuePair<string, Chunk> c in chunks)
        {
            c.Value.DrawChunk();
            yield return null;
        }
    }
    //NOT USED ANYMORE
    IEnumerator BuildWorld()
    {
        int posx = (int)Mathf.Floor(player.transform.position.x / chunkSize);
        int posz = (int)Mathf.Floor(player.transform.position.z / chunkSize);

        float totalChunks = (Mathf.Pow(radius * 2 + 1, 2) * columnHeight) * 2;
        int processCount = 0;


        for (int z = -radius; z <= radius; z++)
        {
            for(int x = -radius; x <= radius; x++)
            {
                for(int y = 0; y < columnHeight; y++)
                {
                    Vector3 chunkPosition = new Vector3((x+posx)*chunkSize, y * chunkSize, (z+posz)*chunkSize);
                    Chunk c;
                    //Get name of Chunk
                    string n = BuildChunkName(chunkPosition);
                    //If chunk already exists within radius in dictionary give it keep status
                    if(chunks.TryGetValue(n, out c))
                    {
                        c.status = Chunk.ChunkStatus.KEEP;
                        //Break because chunk already exists and therefore column exists.
                        break;
                    }
                    else
                    {
                        c = new Chunk(chunkPosition, textureAtlas);
                        c.chunk.transform.parent = this.transform;
                        chunks.TryAdd(c.chunk.name, c);
                    }
                    if (firstbuild)
                    {
                        processCount++;
                        loadingAmount.value = processCount / totalChunks * 100;
                    }
                    yield return null;
                }
            }

        }

        foreach (KeyValuePair<string, Chunk> c in chunks)
        {
            if (c.Value.status == Chunk.ChunkStatus.DRAW)
            {
                c.Value.DrawChunk();
                c.Value.status = Chunk.ChunkStatus.KEEP;
            }
            // deleting old chunks

            c.Value.status = Chunk.ChunkStatus.DONE;
            if (firstbuild)
            {
                processCount++;
                loadingAmount.value = processCount / totalChunks * 100;
            }
            yield return null;
        }
        if (firstbuild)
        {
            player.SetActive(true);
            loadingAmount.gameObject.SetActive(false);
            cam.gameObject.SetActive(false);
            playButton.gameObject.SetActive(false);
            firstbuild = false;
        }
        building = false;
    }

    public void StartBuild()
    {   //Start coroutine queue
        queue = new CoroutineQueue(maxCoroutines, StartCoroutine);
        //Get player position
        Vector3 ppos = player.transform.position;
        //Set the player position to above the highest block
        player.transform.position = new Vector3(ppos.x, Utils.GenerateHeight(ppos.x, ppos.z) + 1, ppos.z);
        lastBuildPos = player.transform.position;
        //Build Chunk around the player
        BuildChunkAt((int)player.transform.position.x / chunkSize, (int)player.transform.position.y / chunkSize, (int)player.transform.position.z / chunkSize);
        //Draw the chunk
        queue.Run(DrawChunks());
        //Start recursive chunk building
        queue.Run(BuildRecursiveWorld((int)player.transform.position.x / chunkSize, (int)player.transform.position.y / chunkSize, (int)player.transform.position.z / chunkSize, radius));


    }
    void preStart()
    {
        firstbuild = false;
        loadingAmount.gameObject.SetActive(false);
        cam.gameObject.SetActive(false);
        playButton.gameObject.SetActive(false);
        player.SetActive(true);
    }
    // Start is called before the first frame update
    void Start()
    {
        player.SetActive(false);
        chunks = new ConcurrentDictionary<string, Chunk>();
        this.transform.position = Vector3.zero;
        this.transform.rotation = Quaternion.identity;

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 movement = lastBuildPos - player.transform.position;
        // If the length of movement is bigger than a chunk then create a new chunk near player
        if (movement.magnitude > Mathf.Floor(radius/2))
        {
            lastBuildPos = player.transform.position;
            BuildNearPlayer();
        }
        queue.Run(DrawChunks());
    }
}
