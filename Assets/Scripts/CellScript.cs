using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellScript : MonoBehaviour {
	SpriteRenderer spriteR;


	public Sprite[] cellSprites;


	// Use this for initialization
	void Awake () {
		spriteR = GetComponent<SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetSprite(int spriteIndex){
		spriteR.sprite = cellSprites [spriteIndex];
	}
}
