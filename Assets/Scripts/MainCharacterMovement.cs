using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    public Node currentNode;
    public Node targetNode;
    public float invDurationPerPellet;
    [HideInInspector]
    public float invincibleTimer = 0f;
    public bool isInvincible = false;
    protected PathFinding pathFinder;
    int ghostScore = 100;

    bool intersect;
    mapGenerator map;
    UIDisplay ui;
    // Use this for initialization
    void Awake()
    {
        map = FindObjectOfType<mapGenerator>();
        ui = FindObjectOfType<UIDisplay>();
        pathFinder = GameObject.FindGameObjectWithTag("pathfinding").GetComponent<PathFinding>();

    }
    void Start () {
        //new code
        currentNode = pathFinder.WorldPosToNode(transform.position);
        targetNode = currentNode;
        intersect = (pathFinder.IsNodeIntersection(targetNode));

        //old code
        dead = false;
        GetComponent<CircleCollider2D>().enabled = true; 
    }


    // Update is called once per frame
    void Update()
    {

        //new code
        if (!dead) {
            transform.position = Vector3.Lerp(currentNode.pos, targetNode.pos, lerpCycle);

        lerpCycle += Time.deltaTime;
        if (lerpCycle >= 1f) {
            //update grid
        }
        if (isInvincible)
        {
            invincibleTimer -= Time.deltaTime;

            if (invincibleTimer <= 0)
            {
                isInvincible = false;
                invincibleTimer = 0;
                ghostScore = 100;
            }
        }



        /*
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
                }*/
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Pellet")
        {
            ui.IncrementScore(1);

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
                ui.IncrementScore(ghostScore);
                ghostScore *= 2;
            }
            else
            {
                mapGenerator mapGen = FindObjectOfType<mapGenerator>();
                mapGen.DecLives();

                GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
                
               
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
