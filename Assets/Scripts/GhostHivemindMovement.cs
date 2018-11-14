using System.Collections;
using System.Collections.Generic;
using UnityEngine;





public class GhostHivemindMovement : MonoBehaviour {

	class GhostData{
		GameObject ghostObject;
		State ghostState;
		Animator ghostAnimator;

		public GhostData(GameObject ghost, State originalState, Animator anim){
			ghostObject = ghost;
			ghostState = originalState;
			ghostAnimator = anim;
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

	public State[] waveStates;
	//Should be of one size lower than waveStates
	public float[] waveEndTimes;

	public float maxVelocity;

	public float frightenedVelocity;

	public float frightenedTime;

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


	}
	
	// Update is called once per frame
	void Update () {
		
	}


	public void BecomeFrightened(){
		foreach (GhostData ghostData in ghostMap.Values) {
			ghostData.setGhostState (State.FRIGHTENED);
		}
	}
}
