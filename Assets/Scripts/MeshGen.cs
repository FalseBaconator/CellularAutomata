using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGen : MonoBehaviour
{

    public SquareGrid grid;

    public void GenerateMesh(int[,] map, float squareSize)
    {
        grid = new SquareGrid(map, squareSize);

    }

    private void OnDrawGizmos()
    {
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

        public Square(ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomLeft, ControlNode _bottomRight)
        {
            topLeft = _topLeft;
            topRight = _topRight;
            bottomLeft = _bottomLeft;
            bottomRight = _bottomRight;

            centreTop = topLeft.right;
            centreRight = bottomRight.above;
            centreBottom = bottomLeft.right;
            centreLeft = bottomLeft.above;
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
