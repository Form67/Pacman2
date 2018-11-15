using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction {  None, Up, Down, Left, Right }

public class PathFinding : MonoBehaviour {
    public List<Node[]> grid;
    int numRows { get { return grid.Count; } }
    int numCols { get { return grid[0].Length; } }
    
    // Intialize our graph
    public void InitGraph(List<GameObject[]> board)
    {
        grid = new List<Node[]>();
        int rowLength = board[0].Length;

        for(int x = 0; x < board.Count; x++)
        {
            grid.Add(new Node[rowLength]);
            for (int y = 0; y < board[0].Length; y++)
            {
                grid[x][y] = new Node(board[x][y], x, y);
            }
        }

    }

    // A-Star path finding algorithm
    public List<Node> AStar(Node start, Node target)
    {
        List<Node> openList = new List<Node>();   // List of discovered nodes that haven't been evaluated yet
        List<Node> closedList = new List<Node>(); // List of nodes that have already been evaluated

        openList.Add(start);    // Add our starting node to openList

        while(openList.Count > 0)
        {

            // Find the node in open list that has the lowest fScore
            Node currentNode = openList[0];
            for(int i = 1; i < openList.Count; i++)
            {
                if(openList[i].fCost < currentNode.fCost ||
                   (openList[i].fCost == currentNode.fCost && openList[i].hCost == currentNode.hCost)){

                    currentNode = openList[i];
                }
            }
            
            // Check if target was reached
            if (start != currentNode && currentNode.Equals(target))
            {
                return ConstructPath(start, target);
            }


            // Adjust lists now that we found the right node 
            openList.Remove(currentNode);
            closedList.Add(currentNode);


            // Visit all the neighbors of the current node
            List<Node> neighbors = GetNeighbors(currentNode);

          
            // Check all the neighbors of the currrentNode
            foreach (Node neighbor in neighbors)
            {
                // Ignore already evaluated neighbor
                if (neighbor.isWall || closedList.Contains(neighbor))
                    continue;

                // Distance from start to neighbor (the f cost)
                int cost = currentNode.gCost + ManhattanDistance(currentNode, neighbor);

                // Just visited a new node
                if (!openList.Contains(neighbor))
                {
                    openList.Add(neighbor);
                }

                // Path is not better
                else if (cost >= neighbor.gCost)
                    continue;
                

                // Path is better
                neighbor.gCost = cost;
                neighbor.hCost = ManhattanDistance(currentNode, neighbor);
                neighbor.parent = currentNode;

            }
        }

        return new List<Node>();    // no path was found
    }

    // Convert node links (parent -> child relationship) into an ordered list 
    List<Node> ConstructPath(Node start, Node target)
    {
        List<Node> path = new List<Node>();
        Node currentNode = target;

        while (!currentNode.Equals(start))
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Add(currentNode);  // add the start node

        path.Reverse();
        return path;
    }

    // Return the manhattan distance between two nodes
    int ManhattanDistance(Node a, Node b)
    {
        return Mathf.Abs(a.gridX - b.gridX) + Mathf.Abs(a.gridY - b.gridY);
    }

    // Neighbors are 1 unit up, down, left, or right from the current node
    public List<Node> GetNeighbors(Node n)
    {
        List<Node> neighbors = new List<Node>();

        // Check up
        if (ValidGridPos(n.gridX, n.gridY + 1))
            neighbors.Add(grid[n.gridX][n.gridY + 1]);

        // Check down
        if (ValidGridPos(n.gridX, n.gridY - 1))
            neighbors.Add(grid[n.gridX][n.gridY - 1]);
        
        // Check left
        if (ValidGridPos(n.gridX - 1, n.gridY))
            neighbors.Add(grid[n.gridX - 1][n.gridY]);

        // Check right
        if (ValidGridPos(n.gridX + 1, n.gridY))
            neighbors.Add(grid[n.gridX + 1][n.gridY]);

        return neighbors;
    }

    // Check if grid coordinates are valid 
    bool ValidGridPos(int x, int y)
    {
        return x >= 0 && x < numRows 
            && y >= 0 && y < numCols;
    }


    // Maps world position to the closest node in the grid
    public Node WorldPosToNode(Vector3 pos)
    {
		Node closest = null;
		float closestDist = float.MaxValue;

        for (int x = 0; x < numRows; x++)
        {
            for (int y = 0; y < numCols; y++)
            {
                float dist = Vector3.Distance(pos, grid[x][y].pos);
                if (dist < closestDist && !grid[x][y].isWall)
                {
                    closest = grid[x][y];
                    closestDist = dist;
                }
            }
        }

        return closest;
    }

	public bool IsNodeIntersection(Node node){
		List<Node> neighbors = GetNeighbors (node);
		int numPathNeighbors = 0;
		foreach (Node neighbor in neighbors) {
			if (!neighbor.isWall) {
				numPathNeighbors++;
			}
		}
		return numPathNeighbors > 2 && !node.isWall;
	}

	public bool IsNodeTurnable(Node node){
		if (node.isWall) {
			return false;
		}
		List<Node> neighbors = GetNeighbors (node);
		List<Node> nonWallNeighbors = new List<Node> ();
		foreach (Node neighbor in neighbors) {
			if (!neighbor.isWall) {
				nonWallNeighbors.Add (neighbor);
			}
		}

		if (nonWallNeighbors.Count > 2) {
			return true;
		} 
		if (nonWallNeighbors.Count <= 1) {
			return false;
		}
		return nonWallNeighbors [0].gridX != nonWallNeighbors [1].gridX && nonWallNeighbors [0].gridY != nonWallNeighbors [1].gridY;

	}

    bool isHouseExit(Node n)
    {
        return ((n.gridY == 13 || n.gridY == 14) && n.gridX == 12);
    }
}
