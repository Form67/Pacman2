﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class MainCharacterMovement : MonoBehaviour {
    public float velocity;
    public int direction;
    public float currVelocity;
    public bool dead;
    public int score;
    public int highScore;
    public Text scoreText;
    public Text scoreText2;

    public float invDurationPerPellet;
    [HideInInspector]
    public float invincibleTimer = 0f;
    public bool isInvincible = false;

    int ghostScore = 100;

    mapGenerator map;
    LivesDisplay livesDisp;

    // Use this for initialization
    void Start () {
        map = FindObjectOfType<mapGenerator>();
        livesDisp = FindObjectOfType<LivesDisplay>();

        dead = false;
        scoreText = GameObject.FindGameObjectWithTag("scoreText").GetComponent<Text>();
        scoreText2 = GameObject.FindGameObjectWithTag("scoreText2").GetComponent<Text>();
        score = 0;
        GetComponent<CircleCollider2D>().enabled = true;

        string path = "Assets/TextFiles/highscore.txt";
        StreamReader reader = new StreamReader(path);
        string parsedText = reader.ReadToEnd().Trim();
        if (parsedText.Length == 0)
            highScore = 0;
        else
            highScore = int.Parse(parsedText);
        scoreText2.text = "Highscore: " +highScore;
        reader.Close();
    }

    // Update is called once per frame
    void Update()
    {
        scoreText.text = "Score: " + score;
        if (score > highScore) {
            highScore = score;
            scoreText2.text = scoreText2.text = "Highscore: " + highScore;
        }
        if (!dead)
        {
            // Power pellets
            if(isInvincible)
            {
                invincibleTimer -= Time.deltaTime;

                if (invincibleTimer <= 0)
                {
                    isInvincible = false;
                    invincibleTimer = 0;
                    ghostScore = 100;
                }
            }

            //input
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(0);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            {
                direction = -1;
                currVelocity = velocity;
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                direction = 1;
                currVelocity = velocity;
                transform.rotation = Quaternion.Euler(0, 0, 90);
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            {
                direction = -1;
                currVelocity = -velocity;
                transform.rotation = Quaternion.Euler(0, 0, 180);
            }
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                direction = 1;
                currVelocity = -velocity;
                transform.rotation = Quaternion.Euler(0, 0, 270);
            }

            if (Mathf.Abs(GetComponent<Rigidbody2D>().velocity.x) < .1f && Mathf.Abs(GetComponent<Rigidbody2D>().velocity.y) < .1f)
            {
                GetComponent<Animator>().SetBool("Moving", false);
            }
            else
            {
                GetComponent<Animator>().SetBool("Moving", true);
            }
            if (currVelocity != 0)
            {
                if (direction < 0)
                    GetComponent<Rigidbody2D>().velocity = new Vector3(currVelocity, 0, 0);
                else
                    GetComponent<Rigidbody2D>().velocity = new Vector3(0, currVelocity, 0);
            }
            else
            {
                GetComponent<Animator>().SetBool("Moving", false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Pellet")
        {
            score++;

            if (collision.gameObject.name.Contains("Power"))
            {
                isInvincible = true;
                invincibleTimer += invDurationPerPellet;   // power pellets stack
            }

            Destroy(collision.gameObject);

        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "ghost")
        {

            if (isInvincible)
            {
                // do invincible behavior
                score += ghostScore;
                ghostScore *= 2;
            }
            else
            {
                mapGenerator mapGen = FindObjectOfType<mapGenerator>();
                mapGen.DecLives();

                GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;

                string path = "Assets/TextFiles/highscore.txt";
                StreamWriter wr = new StreamWriter(path);
                wr.Write(highScore);
                wr.Close();
                dead = true;
                GetComponent<Animator>().SetBool("Dead", true);
                GetComponent<CircleCollider2D>().enabled = false;

                StartCoroutine(Respawn());
            }
        }
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(2.0f);
        FindObjectOfType<mapGenerator>().SoftResetGame();
    }
}
