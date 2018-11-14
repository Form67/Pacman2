using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System.IO;

public class UIDisplay : MonoBehaviour {
    public List<GameObject> lifeSprites;
    public Text scoreText;
    public Text highScoreText;

    int visibleCount = 3;

    [HideInInspector]
    public int score;
    int highScore;

    mapGenerator mapGen;

    int extraLifeThreshold = 10000;

	// Use this for initialization
	void Start () {

        for (int i = 0; i < transform.childCount; i++)
        {
            lifeSprites.Add(transform.GetChild(i).gameObject);
        }

        scoreText = GameObject.FindGameObjectWithTag("scoreText").GetComponent<Text>();
        highScoreText = GameObject.FindGameObjectWithTag("scoreText2").GetComponent<Text>();

        mapGen = FindObjectOfType<mapGenerator>();
    }

    private void Update()
    {
        UpdateScoreText(score);
    }

    public void ReadHighScore()
    {

        string path = "Assets/TextFiles/highscore.txt";
        StreamReader reader = new StreamReader(path);
        string parsedText = reader.ReadToEnd().Trim();
        if (parsedText.Length == 0)
            highScore = 0;
        else
            highScore = int.Parse(parsedText);
        highScoreText.text = "Highscore: " + highScore;
        reader.Close();

        //print("read " + highScore);
    }

    public void SaveHighScore()
    {
        if (score > highScore)
            highScore = score;

        //print("saving " + highScore);

        string path = "Assets/TextFiles/highscore.txt";
        StreamWriter wr = new StreamWriter(path);
        wr.Write(highScore);
        wr.Close();
    }


    // Round has reset
    public void ClearScore()
    {
        scoreText.text = "Score: 0";
        score = 0;

        extraLifeThreshold = 10000;
    }

    public void UpdateScoreText(int score){
        scoreText.text = "Score: " + score;
        if (score > highScore)
        {
            UpdateHighScoreText(score);
        }
    }

    public void IncrementScore(int inc)
    {
        score += 2000;

        // Gain a extra life every 10,000 points
        if (score >= extraLifeThreshold && visibleCount < 3)
        {
            mapGen.IncLife();
            visibleCount++;

            lifeSprites[visibleCount - 1].SetActive(true);

            extraLifeThreshold += extraLifeThreshold;
        }

    }

    public void UpdateHighScoreText(int highScore)
    {
        highScoreText.text = "Highscore: " + highScore;
    }

    public void SetDisplay(int lives)
    {
        //print("lives " + lives);
        for (int i = lives; i < visibleCount; i++)
        {
            GameObject last = lifeSprites[visibleCount - 1];
            visibleCount--;

            last.SetActive(false);
        }
    }

    public void ResetLives()
    {
        foreach (GameObject child in lifeSprites)
            child.SetActive(true);

        visibleCount = 3;
    }

}
