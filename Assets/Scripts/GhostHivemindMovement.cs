using System.Collections;
using System.Collections.Generic;
using UnityEngine;





public class GhostHivemindMovement : MonoBehaviour {

	class GhostData{
		GameObject ghostObject;
		State ghostState;
		Animator ghostAnimator;

		public List<Node> currentPath;

		public GhostData(GameObject ghost, State originalState, Animator anim){
			ghostObject = ghost;
			ghostState = originalState;
			ghostAnimator = anim;
			currentPath = null;
		}

		public GameObject getGhostObject(){
			return ghostObject;
		}

		public State getGhostState(){
			return ghostState;
		}

		public void setGhostState(State state){
			ghostState = state;
		}

		public Animator getGhostAnimator(){
			return ghostAnimator;
		}
	}

	Dictionary<string, GhostData> ghostMap;

	GameObject pacman;
	Node pacmanNode;

	public State[] waveStates;
	//Should be of one size lower than waveStates
	public float[] waveEndTimes;

	public float maxVelocity;

	public float frightenedVelocity;

	float frightenedTime;

	float startTime;

	float currentEndTime;

	PathFinding pathFinder;

	// Use this for initialization
	void Start () {
		ghostMap = new Dictionary<string, GhostData> ();

		GameObject blinky = GameObject.Find("Blinky(Clone)");
		GameObject inky = GameObject.Find("Inky(Clone)");
		GameObject pinky = GameObject.Find("Pinky(Clone)");
		GameObject clyde = GameObject.Find("Clyde(Clone)");

		ghostMap.Add ("blinky", new GhostData (blinky, waveStates [0], blinky.GetComponent<Animator> ()));
		ghostMap.Add ("inky", new GhostData (inky, waveStates [0], inky.GetComponent<Animator> ()));
		ghostMap.Add ("pinky", new GhostData (pinky, waveStates [0], pinky.GetComponent<Animator> ()));
		ghostMap.Add ("clyde", new GhostData (clyde, waveStates [0], clyde.GetComponent<Animator> ()));

		pacman = GameObject.FindGameObjectWithTag ("pacman");
		pacmanNode = pathFinder.WorldPosToNode (pacman.transform.position);
		pathFinder = GameObject.FindGameObjectWithTag ("pathfinding").GetComponent<PathFinding> ();
		frightenedTime = pacman.GetComponent<MainCharacterMovement> ().invincibleTimer;

		currentEndTime = waveEndTimes.Length > 0 ? waveEndTimes [0] : -1f;
	}
	
	// Update is called once per frame
	void Update () {
		pacmanNode = pathFinder.WorldPosToNode (pacman.transform.position);
	}



	public void BecomeFrightened(){
		foreach (GhostData ghostData in ghostMap.Values) {
			ghostData.setGhostState (State.FRIGHTENED);
			ghostData.currentPath = null;
		}
	}

	Node BlinkyDetermineTargetForChase(){
		return pacmanNode;
	}

	Node BlinkyDetermineTargetForScatter(){
		return pathFinder.WorldPosToNode (pathFinder.grid [0] [pathFinder.grid [0].Length - 1].pos);
	}

	Node InkyDetermineTargetForChase(){
		Node pacmanGoalNode = pacmanNode;
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
		Vector3 blinkyPosition = ghostMap ["blinky"].getGhostObject ().transform.position;
		Vector3 goalPoint = blinkyPosition + (2 * (pacmanGoalNode.pos - blinkyPosition));
		return pathFinder.WorldPosToNode (goalPoint);
	}

	Node InkyDetermineTargetForScatter() {
		return pathFinder.WorldPosToNode (pathFinder.grid [pathFinder.grid.Count - 1] [pathFinder.grid [pathFinder.grid.Count - 1].Length - 1].pos);
	}
		
	Node PinkyDetermineTargetForChase(){
		Node pacmanGoalNode = pacmanNode;
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
		return pacmanGoalNode;
	}

	Node PinkyDetermineTargetForScatter(){
		return pathFinder.WorldPosToNode (pathFinder.grid [0] [0].pos);
	}

	Node ClydeDetermineTargetForChase(){
		Node clydeNode = pathFinder.WorldPosToNode (ghostMap ["clyde"].getGhostObject ().transform.position);
		if (Mathf.Pow (pacmanNode.gridX - clydeNode.gridX, 2) + Mathf.Pow (pacmanNode.gridY - clydeNode.gridY, 2) >= 64.0f) {
			return pacmanNode;
		}
		return ClydeDetermineTargetForScatter ();
	}

	Node ClydeDetermineTargetForScatter(){
		return pathFinder.WorldPosToNode (pathFinder.grid [pathFinder.grid.Count - 1] [0].pos);
	}
}
