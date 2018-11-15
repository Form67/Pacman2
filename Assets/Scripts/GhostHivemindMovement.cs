﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;





public class GhostHivemindMovement : MonoBehaviour {

	class GhostData{
		GameObject ghostObject;
		State ghostState;
		Animator ghostAnimator;

        Direction direction;
        float lerpTime;

        Node currentNode;

        string ghostName;

		public GhostData(string name, GameObject ghost, State originalState, Animator anim, PathFinding pathFinder){
            ghostName = name;
            ghostObject = ghost;
			ghostState = originalState;
			ghostAnimator = anim;
            direction = Direction.Up;
            lerpTime = 0f;
            currentNode = pathFinder.WorldPosToNodeIncludingGhostHouse(ghost.transform.position);

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

        public Direction getDirection()
        {
            return direction;
        }
        public void setDirection(Direction newDirection)
        {
            direction = newDirection;
        }
        public float getLerpTime()
        {
            return lerpTime;
        }
        public void setLerpTime(float newLerpTime)
        {
            lerpTime = newLerpTime;
        }
        public Node getCurrentNode()
        {
            return currentNode;
        }
        public void setCurrentNode(Node node)
        {
            currentNode = node;
        }
        public string getName()
        {
            return ghostName;
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

    int currentEndIndex;

	PathFinding pathFinder;

	// Use this for initialization
	void Start () {
		ghostMap = new Dictionary<string, GhostData> ();

		GameObject blinky = GameObject.Find("Blinky(Clone)");
		GameObject inky = GameObject.Find("Inky(Clone)");
		GameObject pinky = GameObject.Find("Pinky(Clone)");
		GameObject clyde = GameObject.Find("Clyde(Clone)");

		ghostMap.Add ("blinky", new GhostData ("blinky", blinky, waveStates [0], blinky.GetComponent<Animator> (), pathFinder));
		ghostMap.Add ("inky", new GhostData ("inky", inky, waveStates [0], inky.GetComponent<Animator> (), pathFinder));
		ghostMap.Add ("pinky", new GhostData ("pinky", pinky, waveStates [0], pinky.GetComponent<Animator> (), pathFinder));
		ghostMap.Add ("clyde", new GhostData ("clyde", clyde, waveStates [0], clyde.GetComponent<Animator> (), pathFinder));

		pacman = GameObject.FindGameObjectWithTag ("pacman");
		pacmanNode = pathFinder.WorldPosToNode (pacman.transform.position);
		pathFinder = GameObject.FindGameObjectWithTag ("pathfinding").GetComponent<PathFinding> ();
		frightenedTime = pacman.GetComponent<MainCharacterMovement> ().invincibleTimer;

		currentEndTime = waveEndTimes.Length > 0 ? waveEndTimes [0] : -1f;
        currentEndIndex = 0;
	}
	
	// Update is called once per frame
	void Update () {
        if(pacman == null)
        {
            pacman = GameObject.FindGameObjectWithTag("pacman");
        }
		pacmanNode = pathFinder.WorldPosToNode (pacman.transform.position);
        

        foreach (GhostData data in ghostMap.Values)
        {
            if (Time.time - startTime >= currentEndTime && currentEndTime != -1f && data.getGhostState() != State.FRIGHTENED)
            {
                currentEndIndex++;
                currentEndTime = waveEndTimes.Length > currentEndIndex ? waveEndTimes[currentEndIndex] : -1f;
                data.setGhostState(waveStates[currentEndIndex]);
                startTime = Time.time;
            }
            else if (Time.time - startTime >= frightenedTime && data.getGhostState() == State.FRIGHTENED)
            {
                data.setGhostState(waveStates[currentEndIndex]);
                startTime = Time.time;
                data.getGhostAnimator().SetBool("flash", false);
            }

            if(data.getLerpTime() > 1f)
            {
                data.setCurrentNode(pathFinder.GetNodeInDirection(data.getCurrentNode(), data.getDirection()));
            }

            if ((pathFinder.IsNodeTurnable(data.getCurrentNode()) || pathFinder.GetNodeInDirection(data.getCurrentNode(), data.getDirection()).isWall) && (data.getLerpTime() > 1f || data.getLerpTime() == 0f))
            {

                switch (data.getGhostState())
                {
                    case State.CHASE:

                        Node targetPoint = DetermineTargetForChase(data.getName());

                        List<Node> currentPath = pathFinder.AStar(data.getCurrentNode(), targetPoint, data.getDirection());

                        data.setDirection(GetDirectionBetweenNodes(data.getCurrentNode(), currentPath[1]));
                        break;
                    case State.FRIGHTENED:
                        currentPath = null;


                        List<Node> neighbors = pathFinder.GetNeighbors(data.getCurrentNode());
                        //List<Vector3> possibleVelocities = new List<Vector3> ();
                        List<Direction> possibleDirections = new List<Direction>();
                        foreach (Node neighbor in neighbors)
                        {
                            if (!neighbor.isWall)
                            {
                                if (neighbor.pos.y > data.getCurrentNode().pos.y)
                                {
                                    //possibleVelocities.Add (Vector3.up * frightenedVelocity);
                                    possibleDirections.Add(Direction.Up);
                                }
                                if (neighbor.pos.y < data.getCurrentNode().pos.y)
                                {
                                    //possibleVelocities.Add (Vector3.down * frightenedVelocity);
                                    possibleDirections.Add(Direction.Down);
                                }
                                if (neighbor.pos.x > data.getCurrentNode().pos.x)
                                {

                                    possibleDirections.Add(Direction.Right);
                                }
                                if (neighbor.pos.x < data.getCurrentNode().pos.x)
                                {
                                    possibleDirections.Add(Direction.Left);
                                }
                            }
                        }

                        data.setDirection(possibleDirections[Random.Range(0, possibleDirections.Count)]);

                        break;
                    case State.SCATTER:
                        Node scatterPoint = GetScatterTarget(data.getName());
                        currentPath = pathFinder.AStar(data.getCurrentNode(), scatterPoint, data.getDirection());
                        data.setDirection(GetDirectionBetweenNodes(data.getCurrentNode(), currentPath[1]));
                        break;
                    default:

                        break;
                }

                if (data.getGhostState() != State.FRIGHTENED)
                {
                    switch (data.getDirection())
                    {

                        case (Direction.Right):
                            data.getGhostAnimator().SetTrigger("goright");
                            break;

                        case (Direction.Left):
                            data.getGhostAnimator().SetTrigger("goleft");
                            break;
                        case (Direction.Up):
                            data.getGhostAnimator().SetTrigger("goup");
                            break;
                        case (Direction.Down):
                            data.getGhostAnimator().SetTrigger("godown");
                            break;
                    }

                }

            }
            data.getGhostObject().transform.position = LerpMovement(data);
            
            data.setLerpTime(data.getLerpTime() + Time.deltaTime * (data.getGhostState() == State.FRIGHTENED ? frightenedVelocity : maxVelocity));
            
        }
	}

    Vector3 LerpMovement(GhostData data)
    {
        Node target = pathFinder.GetNodeInDirection(data.getCurrentNode(), data.getDirection());
        if (data.getLerpTime() > 1f)
        {
            data.setLerpTime(0f);
        }
        return Vector3.Lerp(data.getCurrentNode().pos, target.pos, data.getLerpTime());
    }

    Node DetermineTargetForChase(string ghostName)
    {
        if(ghostName == "blinky")
        {
            return BlinkyDetermineTargetForChase();
        }
        if(ghostName == "inky")
        {
            return InkyDetermineTargetForChase();
        }
        if(ghostName == "pinky")
        {
            return PinkyDetermineTargetForChase();
        }
        if(ghostName == "clyde")
        {
            return ClydeDetermineTargetForChase();
        }
        return null;
    }

    Node GetScatterTarget(string ghostName)
    {
        if (ghostName == "blinky")
        {
            return BlinkyDetermineTargetForScatter();
        }
        if (ghostName == "inky")
        {
            return InkyDetermineTargetForScatter();
        }
        if (ghostName == "pinky")
        {
            return PinkyDetermineTargetForScatter();
        }
        if (ghostName == "clyde")
        {
            return ClydeDetermineTargetForScatter();
        }
        return null;
    }

    public void BecomeFrightened(){
		foreach (GhostData ghostData in ghostMap.Values) {
			ghostData.setGhostState (State.FRIGHTENED);
            ghostData.getGhostAnimator().SetBool("flash", true);
        }
        currentEndTime -= Time.time - startTime;
        startTime = Time.time;
        
        
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

    Direction GetDirectionBetweenNodes(Node first, Node second)
    {

        if (first.gridX == second.gridX)
        {
            if (first.gridY == second.gridY - 1)
            {
                return Direction.Right;
            }
            if (first.gridY == second.gridY + 1)
            {
                return Direction.Left;
            }
        }
        if (first.gridY == second.gridY)
        {
            if (first.gridX == second.gridX - 1)
            {
                return Direction.Down;
            }
            if (first.gridX == second.gridX + 1)
            {
                return Direction.Up;
            }
        }
        return Direction.None;
    }

    Direction FlipDirection(Direction direction)
    {
        switch (direction)
        {
            case (Direction.Up):
                return Direction.Down;
            case (Direction.Down):
                return Direction.Up;
            case (Direction.Left):
                return Direction.Right;
            case (Direction.Right):
                return Direction.Left;
        }
        return Direction.None;
    }
    /*
    void HandleCollisions()
    {
        Node frontNode = pathFinder.GetNodeInDirection(currentNode, direction);

        foreach (UpdatedGhostMovement ghost in ghostsList)
        {
            if (this.currentNode == ghost.currentNode || frontNode == ghost.currentNode)
            {
                ghost.FlipDirection();
                ghost.ResetLerpTime();

                this.FlipDirection();
                this.ResetLerpTime();
            }
        }
    }
    */
}
