using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UpdatedGhostMovement : MonoBehaviour {


	[System.Serializable]
	public enum State {
		DEFAULT,
		CHASE,
		SCATTER,
		FRIGHTENED
	}
		
		


	public State[] waveStates;
	//Should be of one size lower than waveStates
	public float[] waveEndTimes;

	public float maxVelocity;

	public float frightenedVelocity;

	public float frightenedTime;

	State currentState;
	float startTime;
	float currentEndTime;
	float currentEndIndex = 0;

	protected Node targetPoint;

	protected PathFinding pathFinder;

	List<Node> currentPath;


	Rigidbody2D rbody;

    protected GameObject pacman;

	// Use this for initialization
	protected void Start () {
		currentState = waveStates [0];
		currentEndTime = waveEndTimes.Length > 0 ? waveEndTimes [0] : -1f;
		currentEndIndex = 0;
		startTime = Time.time;
        pacman = GameObject.FindGameObjectWithTag("pacman");
		rbody = GetComponent<Rigidbody2D> ();
	}
	
	// Update is called once per frame
	void Update () {

		if (Time.time - startTime >= currentEndTime && currentEndTime != -1f && currentState != State.FRIGHTENED) {
			currentEndIndex++;
			currentEndTime = waveEndTimes.Length > currentEndIndex ? waveEndTimes [currentEndIndex] : -1f;
			currentState = waveStates [currentEndIndex];
			startTime = Time.time;
		} else if (Time.time - startTime >= frightenedTime && currentState == State.FRIGHTENED) {
			currentState = waveStates [currentEndIndex];
			startTime = Time.time;
		}

		Vector3 velocity;
		switch (currentState) {
		case State.CHASE:
			DetermineTargetForChase ();
			//A* to targetPoint
			currentPath = pathFinder.AStar (pathFinder.WorldPosToNode (transform.position), targetPoint);
			velocity = PathFollow ();
			break;
		case State.FRIGHTENED:
			Node currentNode = pathFinder.WorldPosToNode (transform.position);
			if (pathFinder.IsNodeIntersection (currentNode)) {
				List<Node> neighbors = pathFinder.GetNeighbors (currentNode);
				List<Vector3> possibleVelocities = new List<Vector3> ();
				foreach (Node neighbor in neighbors) {
					if (!neighbor.isWall) {
						if (neighbor.pos.y > currentNode.pos.y && rbody.velocity.normalized != Vector2.up) {
							possibleVelocities.Add (Vector3.up * frightenedVelocity);
						}
						if (neighbor.pos.y < currentNode.pos.y && rbody.velocity.normalized != Vector2.down) {
							possibleVelocities.Add (Vector3.down * frightenedVelocity);
						}
						if (neighbor.pos.x > currentNode.pos.x && rbody.velocity.normalized != Vector2.right) {
							possibleVelocities.Add (Vector3.right * frightenedVelocity);
						}
						if (neighbor.pos.x < currentNode.pos.x && rbody.velocity.normalized != Vector2.left) {
							possibleVelocities.Add (Vector3.left * frightenedVelocity);
						}
					}
				}
				velocity = possibleVelocities [Random.Range (0, possibleVelocities.Count)];
			} else {
				velocity = rbody.velocity.normalized * frightenedVelocity;	
			}
			break;
		case State.SCATTER:
			GetScatterTarget ();
			currentPath = pathFinder.AStar (pathFinder.WorldPosToNode (transform.position), targetPoint);
			velocity = PathFollow ();
			break;
		default:
			velocity = Vector3.zero;
			break;
		}
		rbody.velocity = velocity;
	}

	Vector3 PathFollow(){
		return StaticSeek (transform.position, currentPath [1].pos);
	}

	Vector3 StaticSeek(Vector3 position, Vector3 target){
		return (target - position).normalized * maxVelocity;
	}

	protected void SetTargetPointPoint (Node point){
		targetPoint = point;
	}

	public void BecomeFrightened(){
		currentEndTime -= Time.time - startTime;
		startTime = Time.time;
		currentState = State.FRIGHTENED;

	}

	abstract protected void DetermineTargetForChase ();

	abstract protected void GetScatterTarget ();

}
