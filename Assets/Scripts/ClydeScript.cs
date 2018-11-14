using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClydeScript : UpdatedGhostMovement {

	protected override void DetermineTargetForChase() {

		Node pacmanNode = pathFinder.WorldPosToNode(pacman.transform.position);
		Node clydeNode = pathFinder.WorldPosToNode (transform.position);
		if (Mathf.Pow (pacmanNode.gridX - clydeNode.gridX, 2) + Mathf.Pow (pacmanNode.gridY - clydeNode.gridY, 2) >= 64.0f) {
			targetPoint = pacmanNode;
		} else {
			GetScatterTarget ();
		}

	}

	protected override void GetScatterTarget() {
		targetPoint = pathFinder.WorldPosToNode (pathFinder.grid [pathFinder.grid.Count - 1] [0].pos);
	}
}
