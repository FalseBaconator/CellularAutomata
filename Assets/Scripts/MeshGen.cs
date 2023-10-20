using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class MeshGen : MonoBehaviour
{
    public MeshFilter walls;
    public SquareGrid grid;
    public MeshFilter cave;

    public bool is2D;

    List<Vector3> vertices;
    List<int> triangles;

    Dictionary<int, List<Triangle>> DictTriangles = new Dictionary<int, List<Triangle>>();

    List<List<int>> outlines = new List<List<int>>();
    HashSet<int> checkedVertices = new HashSet<int>();


    public void GenerateMesh(int[,] map, float squareSize)
    {
        outlines.Clear();
        checkedVertices.Clear();
        DictTriangles.Clear();

        grid = new SquareGrid(map, squareSize);

        vertices = new List<Vector3>();
        triangles = new List<int>();

        for (int i = 0; i < grid.squares.GetLength(0); i++)
        {
            for (int j = 0; j < grid.squares.GetLength(1); j++)
            {
                TriangulateSquare(grid.squares[i, j]);
            }
        }

        Mesh mesh = new Mesh();
        cave.mesh = mesh;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        int tileAmount = 10;
        Vector2[] uvs = new Vector2[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            float percentX = Mathf.InverseLerp(-map.GetLength(0) / 2 * squareSize, map.GetLength(0) / 2 * squareSize, vertices[i].x);
            float percentY = Mathf.InverseLerp(-map.GetLength(1) / 2 * squareSize, map.GetLength(1) / 2 * squareSize, vertices[i].z);
            uvs[i] = new Vector2(percentX * tileAmount, percentY * tileAmount);
        }

        mesh.uv = uvs;

        if (is2D)
        {
            Generate2DColls();
        }
        else { 
            CreateWallMesh();
        }
    }

    void Generate2DColls()
    {

        EdgeCollider2D[] currentColls = gameObject.GetComponents<EdgeCollider2D>();
        for (int i = 0; i < currentColls.Length; i++)
        {
            Destroy(currentColls[i]);
        }

        CalculateMeshOutlines();

        foreach(List<int> outline in outlines)
        {
            EdgeCollider2D edgeColl = gameObject.AddComponent<EdgeCollider2D>();
            Vector2[] edgePoints = new Vector2[outline.Count];
            for (int i = 0;i < outline.Count;i++)
            {
                edgePoints[i] = new Vector2(vertices[outline[i]].x, vertices[outline[i]].z);
            }
            edgeColl.points = edgePoints;
        }
    }

    void CreateWallMesh()
    {
        CalculateMeshOutlines();

        List<Vector3> wallVertices = new List<Vector3>();
        List<int> wallTriangles = new List<int>();
        Mesh wallMesh = new Mesh();
        float wallHeight = 5;

        foreach (List<int> outline in outlines)
        {
            for (int i = 0; i < outline.Count - 1; i++)
            {
                int startIndex = wallVertices.Count;
                wallVertices.Add(vertices[outline[i]]); // left v
                wallVertices.Add(vertices[outline[i+1]]); // right v
                wallVertices.Add(vertices[outline[i]] - Vector3.up * wallHeight); // bottom left v
                wallVertices.Add(vertices[outline[i+1]] - Vector3.up * wallHeight); // bottom right v

                wallTriangles.Add(startIndex + 0);
                wallTriangles.Add(startIndex + 2);
                wallTriangles.Add(startIndex + 3);

                wallTriangles.Add(startIndex + 3);
                wallTriangles.Add(startIndex + 1);
                wallTriangles.Add(startIndex + 0);
            }
        }
        wallMesh.vertices = wallVertices.ToArray();
        wallMesh.triangles = wallTriangles.ToArray();
        walls.mesh = wallMesh;

        Destroy(walls.GetComponent<MeshCollider>());
        MeshCollider wallColl = walls.gameObject.AddComponent<MeshCollider>();
        wallColl.sharedMesh = walls.mesh;

    }

    void TriangulateSquare(Square square)
    {
        switch (square.configuration)
        {
            case 0:
                break;
            case 1:
                MeshFromPoints(square.centreLeft, square.centreBottom, square.bottomLeft);
                break;
            case 2:
                MeshFromPoints(square.bottomRight, square.centreBottom, square.centreRight);
                break;
            case 3:
                MeshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 4:
                MeshFromPoints(square.topRight, square.centreRight, square.centreTop);
                break;
            case 5:
                MeshFromPoints(square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
                break;
            case 6:
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
                break;
            case 7:
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 8:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreLeft);
                break;
            case 9:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
                break;
            case 10:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;
            case 11:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
                break;
            case 12:
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreLeft);
                break;
            case 13:
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
                break;
            case 14:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;
            case 15:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
                checkedVertices.Add(square.topLeft.vertexIndex);
                checkedVertices.Add(square.topRight.vertexIndex);
                checkedVertices.Add(square.bottomRight.vertexIndex);
                checkedVertices.Add(square.bottomLeft.vertexIndex);
                break;

        }
    }

    void MeshFromPoints(params Node[] points)
    {
        AssignVertices(points);
        if(points.Length >= 3)
        {
            CreateTriangle(points[0], points[1], points[2]);
        }
        if(points.Length >= 4)
        {
            CreateTriangle(points[0], points[2], points[3]);
        }
        if(points.Length >= 5)
        {
            CreateTriangle(points[0], points[3], points[4]);
        }
        if(points.Length >= 6)
        {
            CreateTriangle(points[0], points[4], points[5]);
        }
    }

    void AssignVertices(Node[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].vertexIndex == -1)
            {
                points[i].vertexIndex = vertices.Count;
                vertices.Add(points[i].position);
            }
        }
    }

    void CreateTriangle(Node a, Node b, Node c) 
    {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);

        Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);

        AddTriangleToDictionary(triangle.vertexIndexA, triangle);
        AddTriangleToDictionary(triangle.vertexIndexB, triangle);
        AddTriangleToDictionary(triangle.vertexIndexC, triangle);
    }

    void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle)
    {
        if (DictTriangles.ContainsKey(vertexIndexKey))
        {
            DictTriangles[vertexIndexKey].Add(triangle);
        }
        else
        {
            List<Triangle> triangleList = new List<Triangle> { triangle };
            DictTriangles.Add(vertexIndexKey, triangleList);
        }
    }

    int GetConnectedOutlineVertex(int vertexIndex)
    {
        List<Triangle> vertexTriangles = DictTriangles[vertexIndex];
        for (int i = 0; i < vertexTriangles.Count; i++)
        {
            Triangle triangle = vertexTriangles[i];
            for (int j = 0; j < 3; j++)
            {
                int vertexB = triangle[j];
                if (vertexB != vertexIndex && !checkedVertices.Contains(vertexB))
                {
                    if (IsOutlineEdge(vertexIndex, vertexB))
                    {
                        return vertexB;
                    }
                }
            }
        }
        return -1;
    }

    void CalculateMeshOutlines()
    {
        for (int i = 0; i < vertices.Count; i++)
        {
            if (!checkedVertices.Contains(i))
            {
                int newOutlineVertex = GetConnectedOutlineVertex(i);
                if(newOutlineVertex != -1)
                {
                    checkedVertices.Add(i);

                    List<int> newOutline = new List<int>();
                    newOutline.Add(i);
                    outlines.Add(newOutline);
                    FollowOutline(newOutlineVertex, outlines.Count - 1);
                    outlines[outlines.Count - 1].Add(i);
                }
            }
        }
    }

    void FollowOutline(int vertexIndex, int outlineIndex)
    {
        outlines[outlineIndex].Add(vertexIndex);
        checkedVertices.Add(vertexIndex);
        int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);
        if(nextVertexIndex != -1)
        {
            FollowOutline(nextVertexIndex, outlineIndex);
        }
    }

    bool IsOutlineEdge(int vertexA, int vertexB)
    {
        List<Triangle> ATriangles = DictTriangles[vertexA];
        int sharedTriangleCount = 0;

        for (int i = 0; i < ATriangles.Count; i++)
        {
            if (ATriangles[i].Contains(vertexB))
            {
                sharedTriangleCount++;
                if (sharedTriangleCount > 1) break;
            }
        }
        return sharedTriangleCount == 1;
    }

    struct Triangle
    {
        public int vertexIndexA;
        public int vertexIndexB;
        public int vertexIndexC;

        int[] vertices;

        public Triangle (int a, int b, int c)
        {
            vertexIndexA = a;
            vertexIndexB = b;
            vertexIndexC = c;
            vertices = new int[3] { vertexIndexA, vertexIndexB, vertexIndexC };
        }

        public int this[int i]
        {
            get { return vertices[i]; }
        }

        public bool Contains(int vertexIndex)
        {
            return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
        }
    }

    public class SquareGrid
    {
        public Square[,] squares;

        public SquareGrid(int[,] map, float squareSize)
        {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            ControlNode[,] cNodes = new ControlNode[nodeCountX, nodeCountY];

            for (int i = 0; i < nodeCountX; i++)
            {
                for (int j = 0; j < nodeCountY; j++)
                {
                    Vector3 pos = new Vector3(-mapWidth / 2 + i * squareSize + squareSize / 2, 0, -mapHeight / 2 + j * squareSize + squareSize / 2);
                    cNodes[i, j] = new ControlNode(pos, map[i, j] == 1, squareSize);
                }
            }

            squares = new Square[nodeCountX-1, nodeCountY-1];
            for (int i = 0; i < nodeCountX-1; i++)
            {
                for (int j = 0; j < nodeCountY-1; j++)
                {
                    squares[i,j] = new Square(cNodes[i, j+1], cNodes[i+1, j+1], cNodes[i+1,j], cNodes[i,j]);
                }
            }
        }
    }

    public class Square
    {
        public ControlNode topLeft, topRight, bottomLeft, bottomRight;
        public Node centreTop, centreRight, centreBottom, centreLeft;
        public int configuration; //0 through 15

        public Square(ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft)
        {
            topLeft = _topLeft;
            topRight = _topRight;
            bottomLeft = _bottomLeft;
            bottomRight = _bottomRight;

            centreTop = topLeft.right;
            centreRight = bottomRight.above;
            centreBottom = bottomLeft.right;
            centreLeft = bottomLeft.above;

            configuration = 0;
            if(topLeft.active) configuration += 8;
            if(topRight.active) configuration += 4;
            if(bottomRight.active) configuration += 2;
            if(bottomLeft.active) configuration += 1;
        }
    }

    public class Node
    {
        public Vector3 position;
        public int vertexIndex = -1;

        public Node(Vector3 _pos)
        {
            position = _pos;
        }
    }

    public class ControlNode : Node
    {
        public bool active;
        public Node above, right;

        public ControlNode(Vector3 _pos, bool _active, float squareSize) : base(_pos)
        {
            active = _active;
            above = new Node(position + Vector3.forward * squareSize/2);
            right = new Node(position + Vector3.right * squareSize/2);
        }
    }
}
