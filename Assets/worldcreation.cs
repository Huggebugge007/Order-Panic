using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class worldcreation : MonoBehaviour
{
    [Header("Tiles")]
    public Tile grassTile;
    public Tile dirtTile;

    public Tile topHalfLeftTile;
    public Tile bottomHalfLeftTile;
    public Tile topHalfRightTile;
    public Tile bottomHalfRightTile;

    public Tile connectionPieceLeftTile;
    public Tile connectionPieceRightTile;
    public PhysicsMaterial2D groundmat;

    [Header("World")]
    public float startingheight;
    public int height;
    public int width;

    int fakewidth;

    public Transform player;


    [Header("Chunks")]
    public int chunksize = 16;
    public int renderdistance = 3;
    public float cellSize = 0.32f;


    // Store the Chunk component directly instead of the Transform,
    // so we don't need GetComponent<Chunk>() every time we touch a chunk.
    Dictionary<Vector2Int, Chunk> chunks = new();


    GameObject worldparent;


    // Batching buffers used while generating, so we call SetTiles once
    // per chunk instead of SetTile once per block.
    Dictionary<Vector2Int, List<Vector3Int>> pendingPositions = new();
    Dictionary<Vector2Int, List<TileBase>> pendingTiles = new();



    private void Start()
    {
        if (player == null)
        {
 

            enabled = false;
            return;
        }

        EnsureWorldParent();
        RebuildChunkDictionary();

        Vector2Int current =
            GetPlayerChunkCoord(player.position);

        UpdateVisibility(current);
        lastChunk = current;


    }



    void LateUpdate()
    {
        Vector2Int current =
            GetPlayerChunkCoord(player.position);




        if (current != lastChunk)
        {

            UpdateVisibility(current);

            lastChunk = current;
        }
    }


    Vector2Int lastChunk;



    // -----------------------------
    // CHUNK VISIBILITY
    // -----------------------------

    void UpdateVisibility(Vector2Int playerChunk)
    {

        HashSet<Vector2Int> neededChunks = new();


        for (int x = -renderdistance; x <= renderdistance; x++)
        {
            for (int y = -renderdistance; y <= renderdistance; y++)
            {
                Vector2Int coord =
                    playerChunk + new Vector2Int(x, y);


                neededChunks.Add(coord);


                Chunk chunk =
                    GetOrCreateChunk(coord);




                chunk.gameObject.SetActive(true);
            }
        }


        foreach (var chunk in chunks)
        {
            if (!neededChunks.Contains(chunk.Key))
            {

                chunk.Value.gameObject.SetActive(false);
            }
        }
        int active = 0;

        foreach (var chunk in chunks)
        {
            if (chunk.Value.gameObject.activeSelf)
                active++;
        }


    }



    // -----------------------------
    // WORLD GENERATION
    // -----------------------------

    [ContextMenu("Clear World")]
    public void ClearWorld()
    {
        // DestroyImmediate on the root cascades to all children in one
        // call. Doing it this way (rather than deleting a large
        // Hierarchy selection by hand) skips Unity's Undo recording,
        // which is what makes manual deletion of thousands of objects
        // slow.
        if (worldparent != null)
        {
            DestroyImmediate(worldparent);
        }

        chunks.Clear();
        pendingPositions.Clear();
        pendingTiles.Clear();
    }



    [ContextMenu("Generate World")]
    public void GenerateShape()
    {
        chunks.Clear();
        EnsureWorldParent();

        // Tile positions are integer grid cells, but startingheight is a
        // float (e.g. so it can be tuned/animated in world units elsewhere),
        // so round it once here rather than truncating on every row.
        int startY = Mathf.RoundToInt(startingheight);

        fakewidth = width;

        // How far the left edge has shifted from its starting x=0.
        // j (used below for tile-type checks) always runs 0..fakewidth,
        // but we add leftOffset to the actual world position so growth
        // is split evenly between the left and right edges instead of
        // all going to the right.
        int leftOffset = -(width / 2);


        for (int i = 0; i < height; i++)
        {

            if (i == height - 10)
            {
                fakewidth += 1000;
                leftOffset -= 500;
            }


            for (int j = 0; j < fakewidth + 1; j++)
            {
                Vector2Int pos =
                    new Vector2Int(j + leftOffset, startY - i);



                if (i == height - 10)
                {
                    PlaceBlock(grassTile, pos);
                }


                else if (j == 0)
                {
                    PlaceBlock(bottomHalfLeftTile, pos);
                }


                else if (j == 1)
                {
                    PlaceBlock(topHalfLeftTile, pos);
                }


                else if (j == fakewidth - 1)
                {
                    PlaceBlock(topHalfRightTile, pos);
                }


                else if (j == fakewidth)
                {
                    PlaceBlock(bottomHalfRightTile, pos);
                }


                else if (j == 2 && i != 0)
                {
                    PlaceBlock(connectionPieceLeftTile, pos);
                }


                else if (j == fakewidth - 2 && i != 0)
                {
                    PlaceBlock(connectionPieceRightTile, pos);
                }


                else
                {
                    if (i == 0)
                    {
                        PlaceBlock(grassTile, pos);
                    }
                    else
                    {
                        PlaceBlock(dirtTile, pos);
                    }
                }
            }


            fakewidth += 4;
            leftOffset -= 2;
        }


        // Actually push all buffered tiles into their tilemaps now,
        // one SetTiles call per chunk instead of one SetTile per block.
        FlushTiles();

        // Generation just created/activated every chunk it touched
        // (see GetOrCreateChunk), which can span the whole world.
        // Re-run visibility immediately so only chunks near the player
        // stay active, rather than waiting for the player to cross a
        // chunk boundary in LateUpdate.
        Vector2Int current =
            GetPlayerChunkCoord(player.position);

        UpdateVisibility(current);


        lastChunk = current;
    }



    // -----------------------------
    // TILE PLACEMENT (buffered)
    // -----------------------------

    void PlaceBlock(Tile tile, Vector2Int position)
    {
        Vector2Int chunkCoord =
            GetChunkCoord(position, chunksize);


        Vector3Int tilePosition =
            new Vector3Int(
                position.x,
                position.y,
                0
            );


        if (!pendingPositions.TryGetValue(chunkCoord, out List<Vector3Int> posList))
        {
            posList = new List<Vector3Int>();
            pendingPositions[chunkCoord] = posList;
            pendingTiles[chunkCoord] = new List<TileBase>();
        }


        posList.Add(tilePosition);
        pendingTiles[chunkCoord].Add(tile);
    }



    void FlushTiles()
    {
        foreach (var kvp in pendingPositions)
        {
            Chunk chunk =
                GetOrCreateChunk(kvp.Key);

            Vector3Int[] positions =
                kvp.Value.ToArray();

            TileBase[] tiles =
                pendingTiles[kvp.Key].ToArray();

            chunk.tilemap.SetTiles(positions, tiles);
        }


        pendingPositions.Clear();
        pendingTiles.Clear();
    }



    // -----------------------------
    // CHUNK CREATION
    // -----------------------------

    void EnsureWorldParent()
    {
        if (worldparent != null)
            return;

        GameObject existingWorld =
            GameObject.Find("World");

        if (existingWorld != null)
        {
            worldparent = existingWorld;


        }
        else
        {
            worldparent =
                new GameObject("World");

   
        }
    }
    void RebuildChunkDictionary()
    {
        chunks.Clear();

        if (worldparent == null)
        {
  

            return;
        }

        Chunk[] existingChunks =
            worldparent.GetComponentsInChildren<Chunk>(true);

        foreach (Chunk chunk in existingChunks)
        {
            if (chunk == null)
                continue;

            if (chunk.tilemap == null)
            {
      

                continue;
            }

            if (chunks.ContainsKey(chunk.coord))
            {


                continue;
            }

            chunks.Add(chunk.coord, chunk);
        }

    }



    Chunk GetOrCreateChunk(Vector2Int coord)
    {

        if (chunks.TryGetValue(coord, out Chunk existing))
        {

            // Unity's overridden == null check also catches objects
            // that were destroyed (e.g. by exiting Play mode, or by
            // deleting the World object), where the C# reference
            // isn't literally null but the underlying object is gone.
            if (existing != null && existing.tilemap != null)
            {
                return existing;
            }

            chunks.Remove(coord);
        }


        EnsureWorldParent();


        GameObject chunkObject =
            new GameObject(
                "Chunk " + coord
            );


        chunkObject.transform.parent =
            worldparent.transform;

        // Start disabled. Visibility is decided exclusively by
        // UpdateVisibility - a chunk should only ever become active
        // because it was selected as being within render distance,
        // never just because it was created (e.g. during world
        // generation, which can create chunks far from the player).
        chunkObject.SetActive(false);


        // Grid is required for the Tilemap on this chunk to render
        // and align correctly. Cell size must match the tile sprites'
        // world size, or tiles will appear spaced out / overlapping.
        Grid grid = chunkObject.AddComponent<Grid>();
        grid.cellSize = new Vector3(cellSize, cellSize, 0f);



        // Create tilemap
        GameObject tilemapObject =
            new GameObject("Tilemap");


        tilemapObject.transform.parent =
            chunkObject.transform;



        Tilemap tilemap =
            tilemapObject.AddComponent<Tilemap>();


        tilemapObject.AddComponent<TilemapRenderer>();


        // Without a CompositeCollider2D, TilemapCollider2D generates
        // one physics shape PER SOLID TILE. With a mostly-solid island
        // interior across thousands of chunks, that can be hundreds of
        // thousands of shapes - extremely slow to both create and
        // destroy. Marking it "usedByComposite" and adding a
        // CompositeCollider2D merges all of a chunk's shapes into one
        // simplified outline instead.
        TilemapCollider2D collider =
            tilemapObject.AddComponent<TilemapCollider2D>();

        collider.usedByComposite = true;

        Rigidbody2D rb =
            tilemapObject.AddComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Static;

        tilemapObject.AddComponent<CompositeCollider2D>();

        collider.sharedMaterial = groundmat;

        Chunk chunk =
            chunkObject.AddComponent<Chunk>();


        chunk.coord = coord;
        chunk.tilemap = tilemap;



        chunks.Add(
            coord,
            chunk
        );


        return chunk;
    }



    // -----------------------------
    // POSITION CONVERSION
    // -----------------------------

    Vector2Int GetChunkCoord(
        Vector2 pos,
        int size
    )
    {
        int x =
            Mathf.FloorToInt(
                pos.x / size
            );


        int y =
            Mathf.FloorToInt(
                pos.y / size
            );


        return new Vector2Int(x, y);
    }



    // GetChunkCoord above expects a position already in tile-grid units
    // (e.g. the Vector2Int positions used during generation). player.position
    // is in world units (meters), and one tile is cellSize world units, not
    // 1 world unit - so it has to be converted into grid space first, or the
    // chunk index it computes doesn't line up with the chunks generation
    // actually created. Using GetChunkCoord directly on player.position was
    // the cause of chunks appearing to never update as the player moved.
    Vector2Int GetPlayerChunkCoord(Vector3 worldPos)
    {
        Vector2 gridPos =
            new Vector2(
                worldPos.x / cellSize,
                worldPos.y / cellSize
            );

        return GetChunkCoord(gridPos, chunksize);
    }
}