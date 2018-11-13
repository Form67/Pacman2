using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivesDisplay : MonoBehaviour {
    public List<GameObject> children;

    int visibleCount = 3;

	// Use this for initialization
	void Start () {

        for (int i = 0; i < transform.childCount; i++)
        {
            children.Add(transform.GetChild(i).gameObject);
        }
        
	}
	
    public void SetDisplay(int lives)
    {
        for(int i = lives; i < visibleCount; i++)
        {
            GameObject last = children[visibleCount - 1];
            visibleCount--;

            last.SetActive(false);
        }
    }

    public void ResetLives()
    {
        foreach (GameObject child in children)
            child.SetActive(true);
    }
}
