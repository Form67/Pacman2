using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlinkyScript : UpdatedGhostMovement {

	protected override void DetermineTargetForChase() {
		
		Node pacmanGoalNode = pathFinder.WorldPosToNode(pacman.transform.position);
		targetPoint = pacmanGoalNode;
	}

	protected override void GetScatterTarget() {
		targetPoint = pathFinder.WorldPosToNode (pathFinder.grid [0] [pathFinder.grid [0].Length - 1].pos);
	}
}
