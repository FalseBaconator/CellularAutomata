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
                if(i >= 0 && i < width && j >= 0 && j < height)
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
