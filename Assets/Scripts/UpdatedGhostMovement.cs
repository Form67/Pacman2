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


	protected struct Point{
		public int gridX;
		public int gridY;
	}

	public State[] waveStates;
	//Should be of one size lower than waveStates
	public float[] waveEndTimes;

	State currentState;
	float startTime;
	float currentEndTime;

	Point targetPoint;

	PathFinding pathFinder;

	// Use this for initialization
	void Start () {
		currentState = waveStates [0];
		currentEndTime = waveEndTimes.Length > 0 ? waveEndTimes [0] : -1f;
		startTime = Time.time;

	}
	
	// Update is called once per frame
	void Update () {
		switch (currentState) {
		case State.CHASE:
			DetermineTargetForChase ();
			//A* to targetPoint
			break;
		case State.FRIGHTENED:

			break;
		case State.SCATTER:
			GetScatterTarget ();
			break;
		default:
			break;
		}
	}

	protected void SetTargetPointPoint (Point point){
		targetPoint = point;
	}

	abstract protected void DetermineTargetForChase ();

	abstract protected void GetScatterTarget ();

}
