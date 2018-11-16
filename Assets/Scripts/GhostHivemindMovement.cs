using System.Collections;
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
        bool respawn;

        Vector3 originalPos;

        float exitTime;

		public GhostData(string name, GameObject ghost, State originalState, Animator anim, PathFinding pathFinder, float eTime){
            ghostName = name;
            ghostObject = ghost;
			ghostState = originalState;
			ghostAnimator = anim;
            direction = Direction.Up;
            lerpTime = 0f;
            currentNode = pathFinder.WorldPosToNodeIncludingGhostHouse(ghost.transform.position);
            originalPos = ghost.transform.position;
            exitTime = eTime;

            respawn = false;
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

        public bool getRespawn()
        {
            return respawn;
        }

        public void setRespawn(bool b)
        {
            respawn = b;
        }
        
        public Vector3 getOriginalPos()
        {
            return originalPos;
        }

        public float getExitTime()
        {
            return exitTime;
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
    float startExitTime;

    float currentEndTime;

    int currentEndIndex;

	PathFinding pathFinder;
    bool initialized = false;

	// Use this for initialization
	public void Init () {
        print("init");

        pathFinder = GameObject.FindGameObjectWithTag("pathfinding").GetComponent<PathFinding>();
        ghostMap = new Dictionary<string, GhostData> ();

		GameObject blinky = GameObject.Find("Blinky 1(Clone)");
		GameObject inky = GameObject.Find("Inky 1(Clone)");
		GameObject pinky = GameObject.Find("Pinky 1(Clone)");
		GameObject clyde = GameObject.Find("Clyde 1(Clone)");

        if (blinky == null)
            print("blinky?");

        ghostMap.Add("blinky", new GhostData("blinky", blinky, State.DEFAULT, blinky.GetComponent<Animator>(), pathFinder, 0));
        ghostMap.Add("inky", new GhostData("inky", inky, State.DEFAULT, inky.GetComponent<Animator>(), pathFinder, 5));
        ghostMap.Add("pinky", new GhostData("pinky", pinky, State.DEFAULT, pinky.GetComponent<Animator>(), pathFinder, 3));
        ghostMap.Add("clyde", new GhostData("clyde", clyde, State.DEFAULT, clyde.GetComponent<Animator>(), pathFinder, 10));

        pacman = GameObject.FindGameObjectWithTag ("pacman");
		pacmanNode = pathFinder.WorldPosToNode (pacman.transform.position);
		frightenedTime = pacman.GetComponent<MainCharacterMovement> ().invincibleTimer;

		currentEndTime = waveEndTimes.Length > 0 ? waveEndTimes [0] : -1f;
        currentEndIndex = 0;

        initialized = true;
        startExitTime = Time.time;

    }
	
	// Update is called once per frame
	void Update () {
        if (!initialized)
            return;

        if(pacman == null)
        {
            pacman = GameObject.FindGameObjectWithTag("pacman");
        }
        
        pacmanNode = pathFinder.WorldPosToNode (pacman.transform.position);

        foreach (GhostData data in ghostMap.Values)
        {
            if (data.getGhostState() != State.DEFAULT)
            {
                if (Time.time - startTime >= currentEndTime && currentEndTime != -1f && data.getGhostState() != State.FRIGHTENED && !data.getRespawn())
                {
                    currentEndIndex++;
                    currentEndTime = waveEndTimes.Length > currentEndIndex ? waveEndTimes[currentEndIndex] : -1f;
                    data.setGhostState(waveStates[currentEndIndex]);
                    startTime = Time.time;
                }
                else if (data.getGhostState() == State.FRIGHTENED)
                {
                    if (Time.time - startTime >= frightenedTime)
                    {
                        data.setGhostState(waveStates[currentEndIndex]);
                        startTime = Time.time;
                        data.getGhostAnimator().SetBool("flash", false);
                    }
                    // End of state is near
                    else if (frightenedTime - (Time.time - startTime) <= 2f)
                    {
                        data.getGhostAnimator().SetBool("flash", true);
                    }
                }

                // Ghost reached respawn
                if (data.getRespawn() == true && data.getCurrentNode() == pathFinder.grid[13][12])
                {
                    GetComponent<SpriteRenderer>().color = Color.white;
                    data.setRespawn(false);
                }


                if (data.getLerpTime() > 1f)
                {
                    data.setCurrentNode(pathFinder.GetNodeInDirection(data.getCurrentNode(), data.getDirection()));
                }

                HandleCollisions(data);

                if ((pathFinder.IsNodeTurnable(data.getCurrentNode(), data.getRespawn()) || pathFinder.GetNodeInDirection(data.getCurrentNode(), data.getDirection()).isWall) && (data.getLerpTime() > 1f || data.getLerpTime() == 0f))
                {
                    switch (data.getGhostState())
                    {
                        case State.CHASE:
                            Node targetPoint;
                            if (data.getRespawn())
                                targetPoint = pathFinder.grid[13][12];
                            else
                                targetPoint = DetermineTargetForChase(data.getName(), data.getRespawn());

                            List<Node> currentPath = pathFinder.AStar(data.getCurrentNode(), targetPoint, data.getDirection());
                            
                            data.setDirection(GetDirectionBetweenNodes(data.getCurrentNode(), currentPath[1]));
                            break;
                        case State.FRIGHTENED:
                            currentPath = null;


                            List<Node> neighbors = pathFinder.GetNeighbors(data.getCurrentNode());
                            List<Direction> possibleDirections = new List<Direction>();
                            foreach (Node neighbor in neighbors)
                            {
                                if (!neighbor.isWall)
                                {
                                    if (neighbor.pos.y > data.getCurrentNode().pos.y && data.getDirection() != Direction.Down)
                                    {
                                        possibleDirections.Add(Direction.Up);
                                    }
                                    if (neighbor.pos.y < data.getCurrentNode().pos.y && data.getDirection() != Direction.Up)
                                    {
                                        possibleDirections.Add(Direction.Down);
                                    }
                                    if (neighbor.pos.x > data.getCurrentNode().pos.x && data.getDirection() != Direction.Left)
                                    {
                                        possibleDirections.Add(Direction.Right);
                                    }
                                    if (neighbor.pos.x < data.getCurrentNode().pos.x && data.getDirection() != Direction.Right)
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

                data.setLerpTime(data.getLerpTime() + Time.deltaTime * (data.getGhostState() == State.FRIGHTENED ? maxVelocity * 0.5f : maxVelocity));
                
            }
            else
            {
                if (startExitTime + data.getExitTime() <= Time.time)
                {
                    data.setGhostState(waveStates[currentEndIndex]);
                }
                if (Time.time - startTime >= currentEndTime && currentEndIndex < waveEndTimes.Length)
                {
                    currentEndIndex++;
                    currentEndTime = waveEndTimes.Length > currentEndIndex ? waveEndTimes[currentEndIndex] : -1f;
                    startTime = Time.time;
                }
            }
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

    Node DetermineTargetForChase(string ghostName, bool respawn)
    {
        if(ghostName == "blinky")
        {
            if (respawn)
                return pathFinder.grid[13][12];
            else
                return BlinkyDetermineTargetForChase();
        }
        if(ghostName == "inky")
        {
            if (respawn)
                return pathFinder.grid[13][12];
            else
                return InkyDetermineTargetForChase();
        }
        if(ghostName == "pinky")
        {
            if (respawn)
                return pathFinder.grid[13][12];
            else
                return PinkyDetermineTargetForChase();
        }
        if(ghostName == "clyde")
        {
            if (respawn)
                return pathFinder.grid[13][12];
            else
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

    public void Eaten(string ghostName)
    {
        GhostData theGhost = ghostMap[ghostName];
        theGhost.setRespawn(true);
        theGhost.getGhostAnimator().SetBool("flash", false);
        theGhost.setGhostState(State.CHASE);
        theGhost.getGhostObject().GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 0.3f);

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
		return pathFinder.WorldPosToNode(pacmanGoalNode.pos);
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


    void HandleCollisions(GhostData ghost)
    {
        Node myNode = ghost.getCurrentNode();
        Direction myDir = ghost.getDirection();
        Node frontNode = pathFinder.GetNodeInDirection(myNode, myDir);

        foreach (GhostData otherGhost in ghostMap.Values)
        {
            if (otherGhost != ghost && !otherGhost.getRespawn() && (myNode == otherGhost.getCurrentNode() || frontNode == otherGhost.getCurrentNode()))
            {
                ghost.setDirection(FlipDirection(myDir));
                ghost.setLerpTime(0);
            }
        }
    }


    public void Reset()
    {
        foreach (GhostData ghost in ghostMap.Values)
        {
            ghost.getGhostObject().transform.position = ghost.getOriginalPos();
            ghost.setDirection(Direction.Up);
            if (ghost.getExitTime() == 0f)
            {
                ghost.setGhostState(waveStates[0]);
            }
            else
                ghost.setGhostState(State.DEFAULT);

            currentEndTime = waveEndTimes.Length > 0 ? waveEndTimes[0] : -1f;
            currentEndIndex = 0;
            startTime = Time.time;
            startExitTime = Time.time;
            ghost.setCurrentNode(pathFinder.WorldPosToNodeIncludingGhostHouse(transform.position));
            ghost.setLerpTime(0f);
        }
    }
}
