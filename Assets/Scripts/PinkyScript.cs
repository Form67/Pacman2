using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinkyScript : UpdatedGhostMovement {

	protected override void DetermineTargetForChase() {
		
		Node pacmanGoalNode = pathFinder.WorldPosToNode(pacman.transform.position);

		if(pacman.transform.eulerAngles.z == 90) {
			if (pacmanGoalNode.gridX > 3) {
				pacmanGoalNode = pathFinder.grid[pacmanGoalNode.gridX - 4][pacmanGoalNode.gridY];
			} else {
				pacmanGoalNode = pathFinder.grid[0][pacmanGoalNode.gridY];
			}
		} else if(pacman.transform.eulerAngles.z == -90) {
			if (pacmanGoalNode.gridX < pathFinder.grid.Count - 4) {
				pacmanGoalNode = pathFinder.grid[pacmanGoalNode.gridX + 4][pacmanGoalNode.gridY];
			} else {
				pacmanGoalNode = pathFinder.grid[pathFinder.grid.Count - 1][pacmanGoalNode.gridY];
			}
		} else if(pacman.transform.eulerAngles.z == 0) { 
			if (pacmanGoalNode.gridY < pathFinder.grid[pacmanGoalNode.gridX].Length - 4) {
				pacmanGoalNode = pathFinder.grid[pacmanGoalNode.gridX][pacmanGoalNode.gridY + 4];
			} else {
				pacmanGoalNode = pathFinder.grid[pacmanGoalNode.gridX][pathFinder.grid[pacmanGoalNode.gridX].Length - 1];
			}
		} else{
			if (pacmanGoalNode.gridY > 3) {
				pacmanGoalNode = pathFinder.grid[pacmanGoalNode.gridX][pacmanGoalNode.gridY - 4];
			} else {
				pacmanGoalNode = pathFinder.grid[pacmanGoalNode.gridX][0];
			}
		}

		targetPoint = pacmanGoalNode;
	}

	protected override void GetScatterTarget() {
		targetPoint = pathFinder.WorldPosToNode (pathFinder.grid [0] [0].pos);
	}
}
