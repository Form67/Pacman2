﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class mapGenerator : MonoBehaviour {

	public string filePath;
	public float cellSize;
	public List<GameObject[]> board;
	List<string[]> tileTypes;
	public float topLeftX;
	public float topLeftY;

	public GameObject wall;
	public GameObject emptyCell;
	public GameObject emptyWallCell;
	public GameObject pellet;
	public GameObject powerPellet;

	public GameObject pacman;
	public GameObject blinky;
	public GameObject inky;
	public GameObject pinky;
	public GameObject clyde;

	private List<string> wallTypes;
	private bool started = false;
	public GameObject level;

    Vector3 originalPacmanPosition;

    PathFinding path;
    UIDisplay ui;
    GhostHivemindMovement hive;

    int lives = 3;
    public int score = 0;

    void Start()
    {
        path = FindObjectOfType<PathFinding>();
        ui = FindObjectOfType<UIDisplay>();
        hive = FindObjectOfType<GhostHivemindMovement>();
    }

    // Use this for initialization
    void Begin () {
		GameObject currLevel = Instantiate (level);
		StreamReader inp_strm = new StreamReader (filePath);
		started = true;
		board = new List<GameObject[]>();
		tileTypes = new List<string[]> ();
		wallTypes = new List<string>(13);
		wallTypes.Add ("VL");
		wallTypes.Add ("VR");
		wallTypes.Add ("HT");
		wallTypes.Add ("HB");
		wallTypes.Add ("W");
		wallTypes.Add ("G");
		int boardHeight = 0;
		while (!inp_strm.EndOfStream) {
			string line = inp_strm.ReadLine ();
			board.Add (new GameObject[line.Length]);
			tileTypes.Add (new string[line.Length]);
			for (int i = 0; i < line.Length; ++i) {
				char c = line [i];
				tileTypes [boardHeight] [i] = ".";
				if (c == 'W') {
					board [boardHeight] [i] = Instantiate (wall, currLevel.transform);
					tileTypes [boardHeight] [i] = "W";
				} else if (c == 'G') {
					board [boardHeight] [i] = Instantiate (wall, currLevel.transform);
					tileTypes [boardHeight] [i] = "G";
				} else if (c == 'w') {
					board [boardHeight] [i] = Instantiate (emptyWallCell, currLevel.transform);
				} else if (c == '.') {
					board [boardHeight] [i] = Instantiate (emptyCell, currLevel.transform);
					GameObject pelletSpawned = Instantiate (pellet, currLevel.transform);
					pelletSpawned.transform.position = new Vector3 (topLeftX + cellSize * i, topLeftY - cellSize * boardHeight);
				} else if (c == 'E') {
					board [boardHeight] [i] = Instantiate (emptyCell, currLevel.transform);
					GameObject pelletSpawned = Instantiate (powerPellet, currLevel.transform);
					pelletSpawned.transform.position = new Vector3 (topLeftX + cellSize * i, topLeftY - cellSize * boardHeight);
				} else if (c == ' ') {
					board [boardHeight] [i] = Instantiate (emptyCell, currLevel.transform);
				} else if (c == 'I') {
					board [boardHeight] [i] = Instantiate (emptyCell, currLevel.transform);
					GameObject inkySpawned = Instantiate (inky, currLevel.transform);
					inkySpawned.transform.position = new Vector3 (topLeftX + cellSize * i, topLeftY - cellSize * boardHeight);
				} else if (c == 'P') {
					board [boardHeight] [i] = Instantiate (emptyCell, currLevel.transform);
					GameObject pinkySpawned = Instantiate (pinky, currLevel.transform);
					pinkySpawned.transform.position = new Vector3 (topLeftX + cellSize * i, topLeftY - cellSize * boardHeight);
				} else if (c == 'B') {
					board [boardHeight] [i] = Instantiate (emptyCell, currLevel.transform);
					GameObject blinkySpawned = Instantiate (blinky, currLevel.transform);
					blinkySpawned.transform.position = new Vector3 (topLeftX + cellSize * i, topLeftY - cellSize * boardHeight);
				} else if (c == 'C') {
					board [boardHeight] [i] = Instantiate (emptyCell, currLevel.transform);
					GameObject clydeSpawned = Instantiate (clyde, currLevel.transform);
					clydeSpawned.transform.position = new Vector3 (topLeftX + cellSize * i, topLeftY - cellSize * boardHeight);
				} else if (c == 'M') {
					board [boardHeight] [i] = Instantiate (emptyCell, currLevel.transform);
					GameObject pacmanSpawned = Instantiate (pacman, currLevel.transform);
                    
					pacmanSpawned.transform.position = new Vector3 (topLeftX + cellSize * i, topLeftY - cellSize * boardHeight);
                    originalPacmanPosition = pacmanSpawned.transform.position;
                } 
				board [boardHeight] [i].transform.position = new Vector3 (topLeftX + cellSize * i, topLeftY - cellSize * boardHeight);


			}
			boardHeight++;
		}

		inp_strm.Close();
        /**/
        OrientWalls ();

        ui.ReadHighScore();


        // Send new board to pathfinding algo
        path.InitGraph(new List<GameObject[]>(board));
        
        if (hive)
            hive.Init();
    }
	
	// Update is called once per frame
	void Update () {
		GameObject pellet = GameObject.FindGameObjectWithTag ("Pellet");
		if (started && pellet == null) {
			ResetLevel ();
		}
	}

	private void OrientWalls(){
		List<int> uncheckedX = new List<int> ();
		List<int> uncheckedY = new List<int> ();
		for (int i = 0; i < board.Count; ++i) {
			for (int j = 0; j < board [i].Length; ++j) {

				if (tileTypes [i] [j] == "W" && board [i] [j].tag == "wall") {

					CellScript cellScript = board [i] [j].GetComponent<CellScript> ();

					if (i > 0 && i < board.Count - 1 && wallTypes.Contains (tileTypes [i - 1] [j]) && wallTypes.Contains (tileTypes [i + 1] [j])) {
						if (j > 0 && !wallTypes.Contains (tileTypes [i] [j - 1])) {
							//Set tile to 25
							cellScript.SetSprite (25);
							tileTypes [i] [j] = "VR";
							continue;
						}
						if (j < board [i].Length - 1 && !wallTypes.Contains (tileTypes [i] [j + 1])) {
							//Set tile to 24
							cellScript.SetSprite (24);
							tileTypes [i] [j] = "VL";
							continue;
						}
					}
					if (j > 0 && j < board [i].Length - 1 && wallTypes.Contains (tileTypes [i] [j - 1]) && wallTypes.Contains (tileTypes [i] [j + 1])) {
						if (i > 0 && !wallTypes.Contains (tileTypes [i - 1] [j])) {
							//Set tile to 14
							cellScript.SetSprite (14);
							tileTypes [i] [j] = "HB";
							continue;
						}
						if (i < board.Count - 1 && !wallTypes.Contains (tileTypes [i + 1] [j])) {
							//Set tile to 20
							cellScript.SetSprite (20);
							tileTypes [i] [j] = "HT";
							continue;
						}
					}
					uncheckedY.Add (i);
					uncheckedX.Add (j);
				} else if (tileTypes[i] [j] == "G" && board [i] [j].tag == "wall") {
					CellScript cellScript = board [i] [j].GetComponent<CellScript> ();
					if (i > 0 && j > 0 && tileTypes [i - 1] [j] == "G" && tileTypes [i] [j - 1] == "G") {
						cellScript.SetSprite (30);
					} else if (i > 0 && j < board [i].Length - 1 && tileTypes [i - 1] [j] == "G" && tileTypes [i] [j + 1] == "G") {
						cellScript.SetSprite (31);
					} else if (i < board.Count - 1 && j < board [i].Length - 1 && tileTypes [i + 1] [j] == "G" && tileTypes [i] [j + 1] == "G") {
						cellScript.SetSprite (29);
					} else if (i < board.Count - 1 && j > 0 && tileTypes [i + 1] [j] == "G" && tileTypes [i] [j - 1] == "G") {
						cellScript.SetSprite (28);
					} else if (i > 0 && tileTypes [i - 1] [j] == "G") {
						cellScript.SetSprite (2);
					} else {
						cellScript.SetSprite (10);
					}
				}

			}
		}
		int index = 0;
		bool changed = false;
		List<string> rightWalls = new List<string> ();
		rightWalls.Add ("HB");
		rightWalls.Add ("HT");
		rightWalls.Add ("SEO");
		rightWalls.Add ("SEI");
		rightWalls.Add ("NEO");
		rightWalls.Add ("NEI");
		List<string> leftWalls = new List<string> ();
		leftWalls.Add ("HB");
		leftWalls.Add ("HT");
		leftWalls.Add ("SWO");
		leftWalls.Add ("SWI");
		leftWalls.Add ("NWO");
		leftWalls.Add ("NWI");
		List<string> upWalls = new List<string> ();
		upWalls.Add ("VL");
		upWalls.Add ("VR");
		upWalls.Add ("SWO");
		upWalls.Add ("SWI");
		upWalls.Add ("SEO");
		upWalls.Add ("SEI");
		List<string> downWalls = new List<string> ();
		downWalls.Add ("VL");
		downWalls.Add ("VR");
		downWalls.Add ("NWO");
		downWalls.Add ("NWI");
		downWalls.Add ("NEO");
		downWalls.Add ("NEI");
		while (uncheckedX.Count > 0 && index < uncheckedX.Count) {
			int j = uncheckedX [index];
			int i = uncheckedY [index];
			CellScript cellScript = board [i] [j].GetComponent<CellScript> ();
			if (i > 0 && j > 0 && ((upWalls.Contains (tileTypes [i - 1] [j]) && leftWalls.Contains (tileTypes [i] [j - 1]))
				|| (upWalls.Contains (tileTypes [i - 1] [j]) && tileTypes [i] [j - 1] == "W") 
				|| (tileTypes [i - 1] [j] == "W" && leftWalls.Contains (tileTypes [i] [j - 1])))) {
				if (i < board.Count - 1 && j < board [i].Length - 1 && tileTypes [i + 1] [j] == "." && tileTypes [i] [j + 1] == ".") {
					//Set tile to 40
					cellScript.SetSprite (40);
					tileTypes [i] [j] = "NEI";

				} else {
					//Set tile to 37
					cellScript.SetSprite (37);
					tileTypes [i] [j] = "NEO";
				}
				changed = true;
				uncheckedX.RemoveAt (index);
				uncheckedY.RemoveAt (index);
			} else if (i > 0 && j < board [i].Length - 1 && ((upWalls.Contains (tileTypes [i - 1] [j]) && rightWalls.Contains (tileTypes [i] [j + 1]))
				|| (upWalls.Contains (tileTypes [i - 1] [j]) && tileTypes [i] [j + 1] == "W") 
				|| (tileTypes [i - 1] [j] == "W" && rightWalls.Contains (tileTypes [i] [j + 1])))) {
				if (i < board.Count - 1 && j > 0 && tileTypes [i + 1] [j] == "." && tileTypes [i] [j - 1] == ".") {
					//Set tile to 41
					cellScript.SetSprite (41);
					tileTypes [i] [j] = "NWI";
				} else {
					//Set tile to 36
					cellScript.SetSprite (36);
					tileTypes [i] [j] = "NWO";
				}
				changed = true;
				uncheckedX.RemoveAt (index);
				uncheckedY.RemoveAt (index);
			} else if (i < board.Count - 1 && j > 0 && ((downWalls.Contains (tileTypes [i + 1] [j]) && leftWalls.Contains (tileTypes [i] [j - 1]))
				|| (downWalls.Contains (tileTypes [i + 1] [j]) && tileTypes [i] [j - 1] == "W") 
				|| (tileTypes [i + 1] [j] == "W" && leftWalls.Contains (tileTypes [i] [j - 1])))) {
				if (i > 0 && j < board [i].Length - 1 && tileTypes [i - 1] [j] == "." && tileTypes [i] [j + 1] == ".") {
					//Set tile to 38
					cellScript.SetSprite (38);
					tileTypes [i] [j] = "SEI";
				} else {
					//Set tile to 35
					cellScript.SetSprite (35);
					tileTypes [i] [j] = "SEO";
				}
				changed = true;
				uncheckedX.RemoveAt (index);
				uncheckedY.RemoveAt (index);
			} else if (i < board.Count - 1 && j < board [i].Length - 1 && ((downWalls.Contains (tileTypes [i + 1] [j]) && rightWalls.Contains (tileTypes [i] [j + 1]))
				|| (downWalls.Contains (tileTypes [i + 1] [j]) && tileTypes [i] [j + 1] == "W") 
				|| (tileTypes [i + 1] [j] == "W" && rightWalls.Contains (tileTypes [i] [j + 1])))){
				if (i > 0 && j > 0 && tileTypes [i - 1] [j] == "." && tileTypes [i] [j - 1] == ".") {
					//Set tile to 39
					cellScript.SetSprite (39);
					tileTypes [i] [j] = "SWI";
				} else {
					//Set tile to 34
					cellScript.SetSprite (34);
					tileTypes [i] [j] = "SWO";
				}
				changed = true;
				uncheckedX.RemoveAt (index);
				uncheckedY.RemoveAt (index);
			} else {
				index++;

			}
			if (changed && index == uncheckedX.Count) {
				index = 0;
				changed = false;
			}
		}
	}

	public void ResetGame(){
        //print("hard reset");
		GameObject currLevel = GameObject.FindGameObjectWithTag ("level");

        if (currLevel != null)
        {
            ui.SaveHighScore();
            Destroy(currLevel);
            score = 0;
        }

        lives = 2;
        ui.ResetLives();
        Begin ();

        ui.ClearScore();

        StartCoroutine(StartSeq());
    }

    IEnumerator StartSeq()
    {
        yield return new WaitForSeconds(0.8f);
        ui.SetDisplay(lives);
    }

    // Do not save or reset scores
    public void SoftResetGame()
    {
        //print("sort reset");
        if (lives < 0)
        {
            ResetGame();
            return;
        }

        ui.SetDisplay(lives);

        ResetAgents();
    }

    void ResetAgents()
    {
        GameObject currLevel = GameObject.FindGameObjectWithTag("level");
        GameObject pacmanSpawned = Instantiate(pacman, currLevel.transform);
        pacmanSpawned.transform.position = originalPacmanPosition;


        if (hive)
            hive.Reset();
        else
        {
            foreach (GameObject ghost in GameObject.FindGameObjectsWithTag("ghost"))
            {
                ghost.GetComponent<UpdatedGhostMovement>().Reset();
            }
        }
    }

	void ResetLevel(){
		GameObject currLevel = GameObject.FindGameObjectWithTag ("level");
		GameObject pacmanSpawned = GameObject.FindGameObjectWithTag ("pacman");
		MainCharacterMovement pacmanScript = pacmanSpawned.GetComponent<MainCharacterMovement> ();
		Destroy (pacmanSpawned);
		GameObject[] ghosts = GameObject.FindGameObjectsWithTag ("ghost");
		foreach (GameObject ghost in ghosts) {
			Destroy (ghost);
		}
		StreamReader inp_strm = new StreamReader (filePath);
		int boardHeight = 0;

        while (!inp_strm.EndOfStream) {
			string line = inp_strm.ReadLine ();
			for (int i = 0; i < line.Length; ++i) {
				char c = line [i];
				if (c == '.') {
					GameObject pelletSpawned = Instantiate (pellet, currLevel.transform);
					pelletSpawned.transform.position = new Vector3 (topLeftX + cellSize * i, topLeftY - cellSize * boardHeight);
				} else if (c == 'I') {
					GameObject inkySpawned = Instantiate (inky, currLevel.transform);
					inkySpawned.transform.position = new Vector3 (topLeftX + cellSize * i, topLeftY - cellSize * boardHeight);
				} else if (c == 'P') {
					GameObject inkySpawned = Instantiate (pinky, currLevel.transform);
					inkySpawned.transform.position = new Vector3 (topLeftX + cellSize * i, topLeftY - cellSize * boardHeight);
				} else if (c == 'B') {
					GameObject inkySpawned = Instantiate (blinky, currLevel.transform);
					inkySpawned.transform.position = new Vector3 (topLeftX + cellSize * i, topLeftY - cellSize * boardHeight);
				} else if (c == 'C') {
					GameObject inkySpawned = Instantiate (clyde, currLevel.transform);
					inkySpawned.transform.position = new Vector3 (topLeftX + cellSize * i, topLeftY - cellSize * boardHeight);
				} else if (c == 'M') {
					
					GameObject newPacman = Instantiate (pacman, currLevel.transform);
					newPacman.transform.position = new Vector3 (topLeftX + cellSize * i, topLeftY - cellSize * boardHeight);
					pacmanScript = newPacman.GetComponent<MainCharacterMovement> ();
				} 
			}
			++boardHeight;

		}
		inp_strm.Close ();

        ui.ReadHighScore();
	}


    public void DecLives()
    {
        lives--;
    }

    public void IncLife()
    {
        lives++;
    }
}
