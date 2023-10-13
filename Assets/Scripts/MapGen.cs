using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGen : MonoBehaviour
{
    public int width;
    public int height;

    public int mapSmoothIter;
    [Range(0, 8)]
    public int mapSmoothSensitivity;

    public string seed;
    public bool randomSeed;

    [Range(0, 100)]
    public int randomFillPercent;

    int[,] map;


    private void Start()
    {
        GenerateMap();
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            GenerateMap();
        }
    }

    private void GenerateMap()
    {
        map = new int[width, height];
        RandomFillMap();
        for (int i = 0; i < mapSmoothIter; i++)
        {
            SmoothMap();
        }

        ProcessMap();

        int borderSize = 5;
        int[,] borderedMap = new int[width + borderSize * 2, height + borderSize * 2];

        for (int i = 0; i < borderedMap.GetLength(0); i++)
        {
            for (int j = 0; j < borderedMap.GetLength(1); j++)
            {
                if(i >= borderSize && i < width + borderSize && j >= borderSize && j < height + borderSize)
                {
                    borderedMap[i, j] = map[i - borderSize, j - borderSize];
                }
                else
                {
                    borderedMap[i, j] = 1;
                }
            }
        }

        MeshGen meshGen = GetComponent<MeshGen>();
        meshGen.GenerateMesh(borderedMap, 1);

    }

    void RandomFillMap()
    {
        if(randomSeed)
        {
            seed = Time.time.ToString();
        }

        System.Random psudoRand = new System.Random(seed.GetHashCode());

        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                if(i == 0 || i == width-1 || j == 0 || j == height-1)
                {
                    map[i, j] = 1;
                }
                else 
                { 
                    map[i, j] = (psudoRand.Next(0, 100) < randomFillPercent) ? 1 : 0;
                }
            }
        }

    }

    void SmoothMap()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                int count = SurroundingWallCount(i, j);
                if(count > mapSmoothSensitivity)
                {
                    map[i, j] = 1;
                }
                else if (count < mapSmoothSensitivity)
                {
                    map[i, j] = 0;
                }
            }
        }
    }

    int SurroundingWallCount(int x, int y)
    {
        int count = 0;

        for (int i = x-1; i <= x + 1; i++)
        {
            for (int j = y-1; j <= y + 1; j++)
            {
                if(IsInMapRange(i, j))
                {
                    if (i != x || j != y)
                    {
                        count += map[i, j];
                    }
                }
                else
                {
                    count++;
                }
            }
        }

        return count;
    }

    void ProcessMap()
    {
        List<List<Coord>> wallRegions = GetRegions(1);
        int wallThresholdSize = 50;
        foreach (List<Coord> wallRegion in wallRegions)
        {
            if(wallRegion.Count < wallThresholdSize)
            {
                foreach (Coord tile in wallRegion)
                {
                    map[tile.tileX, tile.tileY] = 0;
                }
            }
        }

        List<List<Coord>> roomRegions = GetRegions(0);
        int roomThresholdSize = 50;
        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize)
            {
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            }
        }
    }

    List<List<Coord>> GetRegions(int tileType)
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (mapFlags[i, j] == 0 && map[i, j] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(i, j);
                    regions.Add(newRegion);

                    foreach (Coord tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }
        return regions;
    }

    List<Coord> GetRegionTiles(int startX, int startY)
    {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[width, height];
        int tileType = map[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();

        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);
            for (int i = tile.tileX - 1; i <= tile.tileX + 1; i++)
            {
                for (int j = tile.tileY - 1; j <= tile.tileY + 1; j++)
                {
                    if (IsInMapRange(i, j) && (i == tile.tileX || j == tile.tileY)){
                        if (mapFlags[i,j] == 0 && map[i,j] == tileType)
                        {
                            mapFlags[i, j] = 1;
                            queue.Enqueue(new Coord(i, j));
                        }
                    }
                }
            }
        }
        return tiles;
    }

    bool IsInMapRange(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    struct Coord
    {
        public int tileX;
        public int tileY;

        public Coord(int x, int y)
        {
            tileX = x;
            tileY = y;
        }
    }

    private void OnDrawGizmos()
    {
        /*
        if(map != null)
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Gizmos.color = (map[i,j] == 1)?Color.black : Color.white;
                    Vector3 pos = new Vector3(-width / 2 + i + .5f, 0, -height / 2 + j + .5f);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }
        */
    }

}
