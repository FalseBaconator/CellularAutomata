using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGen : MonoBehaviour
{
    public Player player;
    public Player2D player2D;

    public int width;
    public int height;

    public int mapSmoothIter;
    [Range(0, 8)]
    public int mapSmoothSensitivity;

    public string seed;
    public bool randomSeed;

    [Range(0, 100)]
    public int randomFillPercent;

    public int passageWidth;

    int squareSize = 1;

    int[,] map;

    public Exit exit;

    public int borderSize;

    int playerX;
    int playerY;


    private void Start()
    {
        player = FindObjectOfType<Player>();
        player2D = FindObjectOfType<Player2D>();
        GenerateMap();
    }

    public void GenerateMap()
    {
        map = new int[width, height];
        RandomFillMap();
        for (int i = 0; i < mapSmoothIter; i++)
        {
            SmoothMap();
        }

        ProcessMap();
        
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
        meshGen.GenerateMesh(borderedMap, squareSize);

        int tempX = 0;
        int tempZ = 0;
        System.Random rand = new System.Random();
        Vector3 toSpawn;
        do
        {
            tempX = rand.Next(width);
            tempZ = rand.Next(height);
        } while (SurroundingWallCount(tempX, tempZ) > 0);
        toSpawn = CoordToWorld(new Coord(tempX, tempZ));
        if (meshGen.is2D)
        {
            player2D.Spawn(toSpawn.x, toSpawn.z);
        }
        else
        {
            player.Spawn(toSpawn.x, toSpawn.z);
        }

        tempX = 0;
        tempZ = 0;
        do
        {
            tempX = rand.Next(width);
            tempZ = rand.Next(height);
        } while (SurroundingWallCount(tempX, tempZ) > 0 || (Mathf.Abs(tempX - playerX) < 15 && Mathf.Abs(tempZ - playerY) < 15));
        toSpawn = CoordToWorld(new Coord(tempX, tempZ));
        exit.Spawn(toSpawn.x, toSpawn.z);
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
        List<Room> survivingRooms = new List<Room>();

        foreach (List<Coord> roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomThresholdSize)
            {
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            }
            else
            {
                survivingRooms.Add(new Room(roomRegion, map));
            }
        }

        survivingRooms.Sort();
        survivingRooms[0].isMain = true;
        survivingRooms[0].isAccessibleFromMain = true;

        ConnectClosestRooms(survivingRooms);

    }

    void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMain = false)
    {

        List<Room> roomListA = new List<Room>();
        List<Room> roomListB = new List<Room>();

        if (forceAccessibilityFromMain)
        {
            foreach(Room room in allRooms)
            {
                if(room.isAccessibleFromMain)
                {
                    roomListB.Add(room);
                }
                else
                {
                    roomListA.Add(room);
                }
            }
        }
        else
        {
            roomListA = allRooms;
            roomListB = allRooms;
        }

        int bestDistance = 0;
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();
        bool possibleConnection = false;

        foreach (Room roomA in roomListA)
        {
            if (!forceAccessibilityFromMain)
            {
                possibleConnection = false;
                if(roomA.connectedRooms.Count > 0)
                {
                    continue;
                }
            }
            foreach (Room roomB in roomListB)
            {
                if(roomA == roomB || roomA.IsConnected(roomB))
                {
                    continue;
                }
                for (int i = 0; i < roomA.edges.Count; i++)
                {
                    for (int j = 0; j < roomB.edges.Count; j++)
                    {
                        Coord tileA = roomA.edges[i];
                        Coord tileB = roomB.edges[j];
                        int distBetweenRooms = (int)(MathF.Pow(tileA.tileX - tileB.tileX, 2) + MathF.Pow(tileA.tileY - tileB.tileY, 2));

                        if(distBetweenRooms < bestDistance || !possibleConnection)
                        {
                            bestDistance = distBetweenRooms;
                            possibleConnection = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }

            if (possibleConnection && !forceAccessibilityFromMain)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }

        }

        if (forceAccessibilityFromMain && possibleConnection)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(allRooms, true);
        }

        if (!forceAccessibilityFromMain)
        {
            ConnectClosestRooms(allRooms, true);
        }
    }

    void CreatePassage(Room a, Room b, Coord tileA, Coord tileB)
    {
        Room.ConnectRooms(a, b);

        List<Coord> line = GetLine(tileA, tileB);
        foreach (Coord c in line)
        {
            DrawCircle(c, passageWidth);
        }
    }

    void DrawCircle(Coord c, int r)
    {
        for (int i = -r; i <= r; i++)
        {
            for (int j = -r; j <= r; j++)
            {
                if(i*i + j*j <= r * r)
                {
                    int drawX = c.tileX + i;
                    int drawY = c.tileY + j;
                    if(IsInMapRange(drawX, drawY))
                    {
                        map[drawX, drawY] = 0;
                    }
                }
            }
        }
    }

    List<Coord> GetLine(Coord from, Coord to)
    {
        List<Coord> line = new List<Coord>();
        int x = from.tileX;
        int y = from.tileY;

        int dx = to.tileX - x;
        int dy = to.tileY - y;

        bool inverted = false;
        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        int longest = Math.Abs(dx);
        int shortest = Math.Abs(dy);

        if(longest < shortest)
        {
            inverted = true;
            longest = Math.Abs(dy);
            shortest = Math.Abs(dx);
            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Coord(x, y));
            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }
            gradientAccumulation += shortest;
            if(gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }
        return line;
    }

    Vector3 CoordToWorld(Coord tile)
    {
        return new Vector3(-width / 2 + .5f + tile.tileX, 2, -height / 2 + .5f + tile.tileY);
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

    class Room : IComparable<Room>
    {
        public List<Coord> tiles;
        public List<Coord> edges;
        public List<Room> connectedRooms;
        public int roomSize;
        public bool isAccessibleFromMain;
        public bool isMain;

        public Room() { }

        public Room(List<Coord> roomTiles, int[,] map)
        {
            tiles = roomTiles;
            roomSize = tiles.Count;
            connectedRooms = new List<Room>();

            edges = new List<Coord>();
            foreach (Coord tile in tiles)
            {
                for (int i = tile.tileX - 1; i < tile.tileX + 1; i++)
                {
                    for (int j = tile.tileY - 1; j < tile.tileY + 1; j++)
                    {
                        if (i == tile.tileX || j == tile.tileY)
                        {
                            if (map[i, j] == 1)
                            {
                                edges.Add(tile);
                            }
                        }
                    }
                }
            }
        }

        public void SetAccessibleFromMain()
        {
            if (!isAccessibleFromMain)
            {
                isAccessibleFromMain = true;
                foreach (Room room in connectedRooms)
                {
                    room.SetAccessibleFromMain();
                }
            }
        }

        public static void ConnectRooms(Room roomA, Room roomB)
        {
            if (roomA.isAccessibleFromMain)
            {
                roomB.SetAccessibleFromMain();
            }else if (roomB.isAccessibleFromMain)
            {
                roomA.SetAccessibleFromMain();
            }
            roomA.connectedRooms.Add(roomB);
            roomB.connectedRooms.Add(roomA);
        }

        public bool IsConnected(Room otherRoom)
        {
            return connectedRooms.Contains(otherRoom);
        }

        public int CompareTo(Room other)
        {
            return other.roomSize.CompareTo(roomSize);
        }

    }

}
