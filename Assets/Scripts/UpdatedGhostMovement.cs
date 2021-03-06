﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum State
{
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
    Direction direction;

    public float closeEnoughDistance;

    public float exitTime;

    State currentState;
    float startTime;
    float startExitTime;
    float currentEndTime;
    int currentEndIndex = 0;

    protected Node targetPoint;

    protected PathFinding pathFinder;

    [HideInInspector]
    public List<Node> currentPath;

    List<UpdatedGhostMovement> ghostsList;

    Rigidbody2D rbody;

    protected GameObject pacman;

    public float lerpTime;
   

    [HideInInspector]
    public Node currentNode;
    [HideInInspector]
    public bool respawn = false;

    Vector3 originalPosition;
    // Use this for initialization
    protected void Start() {
        originalPosition = transform.position;
        direction = Direction.Up;
        if (exitTime == 0f)
        {
            currentState = waveStates[0];
        }
        else
            currentState = State.DEFAULT;
        
        currentEndTime = waveEndTimes.Length > 0 ? waveEndTimes[0] : -1f;
        currentEndIndex = 0;
        startTime = Time.time;
        startExitTime = Time.time;
        lerpTime = 0f;
        pacman = GameObject.FindGameObjectWithTag("pacman");
        //rbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("ghost");
        ghostsList = new List<UpdatedGhostMovement>();
        foreach (GameObject ghost in ghosts) {
            if (ghost != gameObject) {
                ghostsList.Add(ghost.GetComponent<UpdatedGhostMovement>());
            }
        }
        pathFinder = GameObject.FindGameObjectWithTag("pathfinding").GetComponent<PathFinding>();
        frightenedTime = pacman.GetComponent<MainCharacterMovement>().invDurationPerPellet;
        currentNode = pathFinder.WorldPosToNodeIncludingGhostHouse(transform.position);
    }

    // Update is called once per frame
    void Update() {
        if (currentState != State.DEFAULT)
        {
            if (pacman == null)
            {
                pacman = GameObject.FindGameObjectWithTag("pacman");
            }
            if (Time.time - startTime >= currentEndTime && currentEndIndex < waveEndTimes.Length && currentState != State.FRIGHTENED && respawn == false)
            {
                currentEndIndex++;
                currentEndTime = waveEndTimes.Length > currentEndIndex ? waveEndTimes[currentEndIndex] : -1f;
                currentState = waveStates[currentEndIndex];
                startTime = Time.time;
            }
            else if (currentState == State.FRIGHTENED)
            {

                if (Time.time - startTime >= frightenedTime)
                {
                    currentState = waveStates[currentEndIndex];
                    startTime = Time.time;
                    animator.SetBool("flash", false);
                }
                // End of state is near
                else if (frightenedTime - (Time.time - startTime) <= 2f)
                {
                    animator.SetBool("flash", true);
                }
            }

            // Ghost reached respawn
            if (respawn == true && currentNode == pathFinder.grid[13][12])
            {
                GetComponent<SpriteRenderer>().color = Color.white;
                respawn = false;
            }

            if (lerpTime > 1f)
            {
                currentNode = pathFinder.GetNodeInDirection(currentNode, direction);
            }

            HandleCollisions();

            if ((pathFinder.IsNodeTurnable(currentNode, respawn) || pathFinder.GetNodeInDirection(currentNode, direction).isWall) && (lerpTime > 1f || lerpTime == 0f))
            {
                switch (currentState)
                {
                    case State.CHASE:

                        if (respawn == false)
                            DetermineTargetForChase();

                        currentPath = pathFinder.AStar(currentNode, targetPoint, direction);
                        direction = GetDirectionBetweenNodes(currentNode, currentPath[1]);
                        break;
                    case State.FRIGHTENED:
                        currentPath = null;

                        List<Node> neighbors = pathFinder.GetNeighbors(currentNode);
                        
                        List<Direction> possibleDirections = new List<Direction>();
                        foreach (Node neighbor in neighbors)
                        {
                            if (!neighbor.isWall)
                            {
                                if (neighbor.pos.y > currentNode.pos.y && direction != Direction.Down)
                                {
                                    possibleDirections.Add(Direction.Up);
                                }
                                if (neighbor.pos.y < currentNode.pos.y && direction != Direction.Up)
                                {
                                    possibleDirections.Add(Direction.Down);
                                }
                                if (neighbor.pos.x > currentNode.pos.x && direction != Direction.Left)
                                {
                                    possibleDirections.Add(Direction.Right);
                                }
                                if (neighbor.pos.x < currentNode.pos.x && direction != Direction.Right)
                                {
                                    possibleDirections.Add(Direction.Left);
                                }
                            }
                        }

                        direction = possibleDirections[Random.Range(0, possibleDirections.Count)];

                        break;
                    case State.SCATTER:
                        GetScatterTarget();
                        currentPath = pathFinder.AStar(currentNode, targetPoint, direction);
                 
                        direction = GetDirectionBetweenNodes(currentNode, currentPath[1]);
                        break;
                    default:

                        break;
                }

                if (currentState != State.FRIGHTENED)
                {
                    switch (direction)
                    {
                        case (Direction.Right):
                            animator.SetTrigger("goright");
                            break;
                        case (Direction.Left):
                            animator.SetTrigger("goleft");
                            break;
                        case (Direction.Up):
                            animator.SetTrigger("goup");
                            break;
                        case (Direction.Down):
                            animator.SetTrigger("godown");
                            break;
                    }

                }

            }
            transform.position = PathFollow();

            if (currentState == State.FRIGHTENED)
                lerpTime += Time.deltaTime * maxVelocity * 0.5f;
            else
                lerpTime += Time.deltaTime * maxVelocity;
        }
        else
        {
            if(startExitTime + exitTime <= Time.time)
            {
                currentState = waveStates[currentEndIndex];
            }
            else if (Time.time - startTime >= currentEndTime && currentEndIndex < waveEndTimes.Length )
            {
                currentEndIndex++;
                currentEndTime = waveEndTimes.Length > currentEndIndex ? waveEndTimes[currentEndIndex] : -1f;
                startTime = Time.time;
            }
        }
    }

	Vector3 PathFollow(){
        Node target = pathFinder.GetNodeInDirection(currentNode, direction);
      
		return KinematicSeek (currentNode.pos, target.pos);
	}

	Vector3 StaticSeek(Vector3 position, Vector3 target){
		return (target - position).normalized * maxVelocity;
	}
    Vector3 KinematicSeek(Vector3 position, Vector3 target) {
        if (lerpTime > 1f) {
            lerpTime = 0f;
            
            }
        return Vector3.Lerp(position, target,lerpTime);
    }
	protected void SetTargetPointPoint (Node point){
		targetPoint = point;
	}

	public void BecomeFrightened(){
		currentEndTime -= Time.time - startTime;
		startTime = Time.time;
		currentState = State.FRIGHTENED;
		animator.SetTrigger ("blue");
		
	}
    
    
    protected void HandleCollisions()
    {
        Node frontNode = pathFinder.GetNodeInDirection(currentNode, direction);

        foreach (UpdatedGhostMovement ghost in ghostsList)
        {
            if (!ghost.respawn && (this.currentNode == ghost.currentNode || frontNode == ghost.currentNode))
            {
                this.FlipDirection();
                this.ResetLerpTime();
            }
        }
    }

    void FlipDirection() {
        switch (direction) {
            case (Direction.Up):
                direction = Direction.Down;
                break;
            case (Direction.Down):
                direction = Direction.Up;
                break;
            case (Direction.Left):
                direction = Direction.Right;
                break;
            case (Direction.Right):
                direction = Direction.Left;
                break;
            default:
                break;
        }
        if(pathFinder.GetNodeInDirection(currentNode, direction).isWall)
        {
            direction = Direction.Down;
        }
        if (currentState != State.FRIGHTENED)
        {
            switch (direction)
            {
                case (Direction.Right):
                    animator.SetTrigger("goright");
                    break;
                case (Direction.Left):
                    animator.SetTrigger("goleft");
                    break;
                case (Direction.Up):
                    animator.SetTrigger("goup");
                    break;
                case (Direction.Down):
                    animator.SetTrigger("godown");
                    break;
            }
        }
    }

    Direction GetDirectionBetweenNodes(Node first, Node second) {
       
        if(first.gridX == second.gridX) {
            if(first.gridY == second.gridY - 1) {
                return Direction.Right;
            }
            if(first.gridY == second.gridY + 1) {
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

    public void ResetLerpTime()
    {
        lerpTime = 0;
    }


    public void Eaten()
    {

        respawn = true;
        animator.SetBool("flash", false);
        currentState = State.CHASE;
        targetPoint = pathFinder.grid[13][12];
        GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.3f);
    }

    abstract protected void DetermineTargetForChase ();

	abstract protected void GetScatterTarget ();

    public void Reset()
    {
        transform.position = originalPosition;
        direction = Direction.Up;
        if (exitTime == 0f)
        {
            currentState = waveStates[0];
        }
        else
            currentState = State.DEFAULT;

        currentEndTime = waveEndTimes.Length > 0 ? waveEndTimes[0] : -1f;
        currentEndIndex = 0;
        startTime = Time.time;
        startExitTime = Time.time;
        currentNode = pathFinder.WorldPosToNodeIncludingGhostHouse(transform.position);
        lerpTime = 0f;
    }

}
