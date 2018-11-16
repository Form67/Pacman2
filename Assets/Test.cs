using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Node n = FindObjectOfType<PathFinding>().WorldPosToNode(transform.position);
        print(n.gridY + " " + n.gridX);
    }
}
