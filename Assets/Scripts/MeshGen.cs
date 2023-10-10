using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGen : MonoBehaviour
{

    public SquareGrid grid;
    List<Vector3> vertices;
    List<int> triangles;

    public void GenerateMesh(int[,] map, float squareSize)
    {
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
        GetComponent<MeshFilter>().mesh = mesh;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    void TriangulateSquare(Square square)
    {
        switch (square.configuration)
        {
            case 0:
                break;
            case 1:
                MeshFromPoints(square.centreBottom, square.bottomLeft, square.centreLeft);
                break;
            case 2:
                MeshFromPoints(square.centreRight, square.bottomRight, square.centreBottom);
                break;
            case 3:
                MeshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 4:
                MeshFromPoints(square.centreTop, square.topRight, square.centreRight);
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
    }

    private void OnDrawGizmos()
    {
        /*
        if(grid != null)
        {
            for (int i = 0; i < grid.squares.GetLength(0); i++)
            {
                for (int j = 0; j < grid.squares.GetLength(1); j++)
                {
                    Gizmos.color = (grid.squares[i, j].topLeft.active)?Color.black:Color.white;
                    Gizmos.DrawCube(grid.squares[i, j].topLeft.position, Vector3.one * .4f);

                    Gizmos.color = (grid.squares[i, j].topRight.active) ? Color.black : Color.white;
                    Gizmos.DrawCube(grid.squares[i, j].topRight.position, Vector3.one * .4f);

                    Gizmos.color = (grid.squares[i, j].bottomLeft.active) ? Color.black : Color.white;
                    Gizmos.DrawCube(grid.squares[i, j].bottomLeft.position, Vector3.one * .4f);

                    Gizmos.color = (grid.squares[i, j].bottomRight.active) ? Color.black : Color.white;
                    Gizmos.DrawCube(grid.squares[i, j].bottomRight.position, Vector3.one * .4f);

                    Gizmos.color = Color.grey;
                    Gizmos.DrawCube(grid.squares[i, j].centreTop.position, Vector3.one * .15f);
                    Gizmos.DrawCube(grid.squares[i, j].centreRight.position, Vector3.one * .15f);
                    Gizmos.DrawCube(grid.squares[i, j].centreBottom.position, Vector3.one * .15f);
                    Gizmos.DrawCube(grid.squares[i, j].centreLeft.position, Vector3.one * .15f);
                }
            }

        }*/
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
