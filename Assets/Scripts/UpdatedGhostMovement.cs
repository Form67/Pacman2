using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum State {
	DEFAULT,
	CHASE,
	SCATTER,
	FRIGHTENED
}

public abstract class UpdatedGhostMovement : MonoBehaviour {
		
	public State[] waveStates;
	//Should be of one size lower than waveStates
	public float[] waveEndTimes;

	public float maxVelocity;

	public float frightenedVelocity;

	public float frightenedTime;

	Animator animator;

	public float closeEnoughDistance;

	public int lookAheadIndexesForCollision;

	State currentState;
	float startTime;
	float currentEndTime;
	int currentEndIndex = 0;

	protected Node targetPoint;

	protected PathFinding pathFinder;

	[HideInInspector]
	public List<Node> currentPath;

	List<UpdatedGhostMovement> ghostsList;

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
		animator = GetComponent<Animator> ();
		GameObject[] ghosts = GameObject.FindGameObjectsWithTag ("ghost");
		ghostsList = new List<UpdatedGhostMovement> ();
		foreach (GameObject ghost in ghosts) {
			if (ghost != gameObject) {
				ghostsList.Add (ghost.GetComponent<UpdatedGhostMovement> ());
			}
		}
		pathFinder = GameObject.FindGameObjectWithTag ("pathfinding").GetComponent<PathFinding> ();
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
			animator.SetBool ("flash", false);
		}

		Vector3 velocity;

		Direction direction = Direction.None;
		if (rbody.velocity.normalized == Vector2.up) {
			direction = Direction.Up;
		} else if (rbody.velocity.normalized == Vector2.down) {
			direction = Direction.Down;
		} else if (rbody.velocity.normalized == Vector2.left) {
			direction = Direction.Left;
		} else if (rbody.velocity.normalized == Vector2.right) {
			direction = Direction.Right;
		}

		//if (Vector3.Distance (transform.position, pathFinder.WorldPosToNode (transform.position).pos) < closeEnoughDistance) {
			CheckForFutureCollisions ();
			switch (currentState) {
			case State.CHASE:
				DetermineTargetForChase ();
			
				currentPath = pathFinder.AStar (pathFinder.WorldPosToNode (transform.position), targetPoint, direction);
				velocity = PathFollow ();
				break;
			case State.FRIGHTENED:
				currentPath = null;
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
				currentPath = pathFinder.AStar (pathFinder.WorldPosToNode (transform.position), targetPoint, direction);
				velocity = PathFollow ();
				break;
			default:
				velocity = Vector3.zero;
				break;
			}
			rbody.velocity = velocity;
			if (currentState != State.FRIGHTENED) {
				if (rbody.velocity.x > 0) {
					animator.SetTrigger ("goright");
				} else if (rbody.velocity.x < 0) {
					animator.SetTrigger ("goleft");
				} else if (rbody.velocity.y > 0) {
					animator.SetTrigger ("goup");
				} else {
					animator.SetTrigger ("godown");
				}
			}
		//}
	}

	Vector3 PathFollow(){
		if (currentPath.Count == 0) {
			return Vector3.zero;
		}
		return StaticSeek (transform.position, currentPath.Count > 1 ? currentPath [1].pos : currentPath[0].pos);
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
		animator.SetBool ("flash", true);
	}

	void CheckForFutureCollisions(){
		if (currentPath != null) {
			foreach (UpdatedGhostMovement ghostScript in ghostsList) {
				if (ghostScript.currentPath != null) {
					for (int i = 0; i < lookAheadIndexesForCollision; ++i) {
						for (int j = 0; j < lookAheadIndexesForCollision; ++j) {
							if (currentPath [i] == ghostScript.currentPath [j]) {
								rbody.velocity = -rbody.velocity;
								return;
							}
						}
					}
				}
			}
		}
	}

	abstract protected void DetermineTargetForChase ();

	abstract protected void GetScatterTarget ();

}
