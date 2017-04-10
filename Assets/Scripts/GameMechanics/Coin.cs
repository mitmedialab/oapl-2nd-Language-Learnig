using UnityEngine;
using System.Collections;
using opal;


public class Coin : MonoBehaviour {

	public Vector2 velocity = new Vector2(-4f, 0);
	public opal.GameController gameController;
	private Rigidbody2D rb2d;


	void Awake(){
		//Check if a GameManager has already been assigned to static variable GameManager.instance or if it's still null
		if (GameController.instance == null) {
			//Instantiate gameManager prefab
			Instantiate (gameController);
		}
		gameController = GameController.instance;
		rb2d = GetComponent<Rigidbody2D>();
	}


	
	// Update is called once per frame
	void Update()
	{

	}

	void FixedUpdate()
	{

	}

	void OnTriggerEnter2D (Collider2D other)
	{
		
		if (other.tag == Constants.TAG_PLAYER) {
			gameController.updateGameScore ();
			Destroy (gameObject);
		} else if (other.tag == "Letter") {

			rb2d.gravityScale = 0.0f;
			rb2d.velocity = new Vector2(0.0f,0.0f);
		}
	}


}

