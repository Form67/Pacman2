using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkyScript : UpdatedGhostMovement {

    GameObject blinky;


    protected new void Start() {
        base.Start();
        blinky = GameObject.Find("Blinky(Clone)");
        
    }

    protected override void DetermineTargetForChase() {
        
        Node pacmanGoalNode = pathFinder.WorldPosToNode(pacman.transform.position);
        if(blinky == null)
        {
            blinky = GameObject.Find("Blinky(Clone)");
        }
		if(pacman.transform.eulerAngles.z == 90) {
            if (pacmanGoalNode.gridX > 1) {
				pacmanGoalNode = pathFinder.grid[pacmanGoalNode.gridX - 2][pacmanGoalNode.gridY];
            } else {
				pacmanGoalNode = pathFinder.grid[0][pacmanGoalNode.gridY];
            }
		} else if(pacman.transform.eulerAngles.z == -90) {
			if (pacmanGoalNode.gridX < pathFinder.grid.Count - 2) {
				pacmanGoalNode = pathFinder.grid[pacmanGoalNode.gridX + 2][pacmanGoalNode.gridY];
			} else {
				pacmanGoalNode = pathFinder.grid[pathFinder.grid.Count - 1][pacmanGoalNode.gridY];
			}
		} else if(pacman.transform.eulerAngles.z == 0) { 
			if (pacmanGoalNode.gridY < pathFinder.grid[pacmanGoalNode.gridX].Length - 2) {
				pacmanGoalNode = pathFinder.grid[pacmanGoalNode.gridX][pacmanGoalNode.gridY + 2];
			} else {
				pacmanGoalNode = pathFinder.grid[pacmanGoalNode.gridX][pathFinder.grid[pacmanGoalNode.gridX].Length - 1];
			}
        } else{
			if (pacmanGoalNode.gridY > 1) {
				pacmanGoalNode = pathFinder.grid[pacmanGoalNode.gridX][pacmanGoalNode.gridY - 2];
			} else {
				pacmanGoalNode = pathFinder.grid[pacmanGoalNode.gridX][0];
			}
        }
		Vector3 goalPoint = blinky.transform.position + (2 * (pacmanGoalNode.pos - blinky.transform.position));
		targetPoint = pathFinder.WorldPosToNode (goalPoint);
    }
    
    protected override void GetScatterTarget() {
		targetPoint = pathFinder.WorldPosToNode (pathFinder.grid [pathFinder.grid.Count - 1] [pathFinder.grid [pathFinder.grid.Count - 1].Length - 1].pos);
    }

}
