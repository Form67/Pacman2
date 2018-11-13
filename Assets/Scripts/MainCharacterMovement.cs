using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class MainCharacterMovement : MonoBehaviour {
    public float velocity;
    public float currVelocity;
    public bool dead;
    public int score;
    public int highScore;
    public Text scoreText;
    public Text scoreText2;
    public enum Dir { up, down, left, right };
    public Dir direction;
    public float lerpCycle;
    // Use this for initialization
    void Start () {
        //new code
    


        //old code
        dead = false;
        scoreText = GameObject.FindGameObjectWithTag("scoreText").GetComponent<Text>();
        scoreText2 = GameObject.FindGameObjectWithTag("scoreText2").GetComponent<Text>();
        score = 0;
        GetComponent<CircleCollider2D>().enabled = true;
        string path = "Assets/TextFiles/highscore.txt";
        StreamReader reader = new StreamReader(path);
        highScore = int.Parse(reader.ReadToEnd());
        scoreText2.text = "Highscore: " +highScore;
        reader.Close();
    }

    // Update is called once per frame
    void Update()
    {
        //new code

        lerpCycle += Time.deltaTime;
        if (lerpCycle >= 1f) {
            //update grid
        }


/*
        scoreText.text = "Score: " + score;
        if (score > highScore) {
            highScore = score;
            scoreText2.text = scoreText2.text = "Highscore: " + highScore;
        }
        if (!dead)
        {
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
        }*/
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Pellet")
        {
            score++;
            Destroy(collision.gameObject);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "ghost")
        {
            GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;

            string path = "Assets/TextFiles/highscore.txt";
            StreamWriter wr = new StreamWriter(path);
            wr.Write(highScore);
            wr.Close();
            dead = true;
            GetComponent<Animator>().SetBool("Dead", true);
            GetComponent<CircleCollider2D>().enabled = false;
        }
    }
}
