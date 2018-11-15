using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node {

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
        
        if (tileObj.tag == "wall")
            this.isWall = true;
        else
            this.isWall = false;
    }

    public override bool Equals(System.Object obj)
    {
        if (obj == null)
            return false;
        Node n = obj as Node;
        if ((System.Object)n == null)
            return false;
        return this.pos == n.pos;
    }

    public override int GetHashCode()
    {
        return this.pos.GetHashCode();
    }

    public bool Equals(Node n)
    {
        if ((object)n == null)
            return false;
        return this.pos == n.pos;
    }

    public override string ToString()
    {
        return "(" + gridY + " " + gridX + ")";
    }


}
