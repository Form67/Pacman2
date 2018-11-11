using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding : MonoBehaviour {
    List<Node[]> graph;

    // Use this for initialization
    private void Awake()
    {
        graph = new List<Node[]>();
    }

    public void InitGraph(List<GameObject[]> board)
    {
        int rowLength = board[0].Length;

        for(int x = 0; x < board.Count; x++)
        {
            graph.Add(new Node[rowLength]);
            for (int y = 0; y < board[0].Length; y++)
            {
                graph[x][y] = new Node(board[x][y], x, y);
            }
        }
    }

    void AStar(Node start, Node target)
    {
        List<Node> openList = new List<Node>();
        List<Node> closedList = new List<Node>();

        openList.Add(start);

        while(openList.Count > 0)
        {
            Node currentNode = openList[0];
            for(int i = 1; i < openList.Count; i++)
            {
                if(openList[i].fCost < currentNode.fCost ||
                   (openList[i].fCost == currentNode.fCost && openList[i].hCost == currentNode.hCost)){

                    currentNode = openList[i];
                }
            }
            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if(currentNode == target)
            {
                ConstructPath(start, target);
                break;
            }

            // TODO 

        }
    }

    void ConstructPath(Node start, Node target)
    {

    }

    int Distance(Node a, Node b)
    {
        return Mathf.Abs(a.gridX - b.gridX) + Mathf.Abs(a.gridY - b.gridY);
    }
}
