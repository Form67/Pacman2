using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour {

    // Relative position in the graph
    public int gridX;
    public int gridY;

    public bool isWall;
    public Vector3 pos;

    public Node parent;

    public int gCost; // Cost of moving to the next node
    public int hCost; // Distance from target to this node

    public int fCost { get { return gCost + hCost; } }

    public Node (GameObject tileObj, int gridX, int gridY)
    {
        this.gridX = gridX;
        this.gridY = gridY;
        this.pos = tileObj.transform.position;

        if (tileObj.tag == "Wall")
            this.isWall = true;
        else
            this.isWall = false;
    }
}
