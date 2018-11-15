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


            lerpCycle += Time.deltaTime * velocity;
            if (targetNode != currentNode) {
                GetComponent<Animator>().SetBool("Moving", true);
            }
            else {
                GetComponent<Animator>().SetBool("Moving", false);
            }
            if (lerpCycle >= 1f) {
                currentNode = targetNode;
                lerpCycle = 0;
                switch (direction) { //next tile movement
                    case Dir.right:
                        transform.rotation = Quaternion.Euler(0, 0, 0);
                        if (!pathFinder.grid[currentNode.gridX][currentNode.gridY + 1].isWall) { 
                            targetNode = pathFinder.grid[currentNode.gridX][currentNode.gridY+1];

                         }
                        else {
                            targetNode = currentNode;
                            
                        }
                        break;
                    case Dir.up:
                        transform.rotation = Quaternion.Euler(0, 0, 90);
                        if (!pathFinder.grid[currentNode.gridX - 1][currentNode.gridY].isWall)
                            targetNode = pathFinder.grid[currentNode.gridX - 1][currentNode.gridY];
                        else
                        {
                            targetNode = currentNode;
                        }
                        break;
                    case Dir.left:
                        transform.rotation = Quaternion.Euler(0, 0, 180);
                        if (!pathFinder.grid[currentNode.gridX][currentNode.gridY - 1].isWall)
                            targetNode = pathFinder.grid[currentNode.gridX][currentNode.gridY - 1];
                        else
                        {
                            targetNode = currentNode;
                        }
                        break;
                    case Dir.down:
                        transform.rotation = Quaternion.Euler(0, 0, 270);
                        if (!pathFinder.grid[currentNode.gridX+1][currentNode.gridY].isWall)
                            targetNode = pathFinder.grid[currentNode.gridX+1][currentNode.gridY];
                        else
                        {
                            targetNode = currentNode;
                        }
                        break;
                    default:
                        break;
                }
                intersect = (pathFinder.IsNodeIntersection(targetNode));
            }
        }
        // Power pellets
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


            if ((Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) && !pathFinder.grid[targetNode.gridX][targetNode.gridY+1].isWall)
            {
                direction = Dir.right;
                
            } 
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) && !pathFinder.grid[targetNode.gridX-1][targetNode.gridY].isWall)
            {
                direction = Dir.up;
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) && !pathFinder.grid[targetNode.gridX][targetNode.gridY-1].isWall)
            {
                direction = Dir.left;
            }
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S) && !pathFinder.grid[targetNode.gridX+1][targetNode.gridY].isWall)
            {
                direction = Dir.down;
            }
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Pellet")
        {
            ui.IncrementScore(1);

            if (collision.gameObject.name.Contains("Power"))
            {
                foreach (GameObject g in GameObject.FindGameObjectsWithTag("ghost")) {
                    
                    g.GetComponent<UpdatedGhostMovement>().BecomeFrightened();
                }
                isInvincible = true;
                invincibleTimer += invDurationPerPellet;   // power pellets stack
            }

            Destroy(collision.gameObject);

        }
        if (collision.gameObject.tag == "ghost")
        {
            UpdatedGhostMovement singular = collision.gameObject.GetComponent<UpdatedGhostMovement>();
            //GhostHivemindMovement hivemind = collision.gameObject.GetComponent<GhostHivemindMovement>();

            if (singular && singular.respawn)
                return;

            if (isInvincible)
            {
                // do invincible behavior
                ui.IncrementScore(ghostScore);
                ghostScore *= 2;

                if (singular)
                    singular.Eaten();
                //else if (hivemind)
                //    // do this
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
    private void OnCollisionEnter2D(Collision2D collision)
    {
        
    }

    IEnumerator Respawn()
    {
        yield return new WaitForSeconds(2.0f);
        FindObjectOfType<mapGenerator>().SoftResetGame();
    }
}
