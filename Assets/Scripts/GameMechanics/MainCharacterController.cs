using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

namespace opal
{
	public class MainCharacterController : MonoBehaviour {


		private Rigidbody2D rb2d;
		private SkeletonAnimation skeletonAnimation;
	

		public float maxSpeed = 10f;
		bool facingRight =true;


		// Use this for initialization
		void Start () 
		{
			rb2d = GetComponent<Rigidbody2D> ();
			skeletonAnimation = GetComponent<SkeletonAnimation> ();
		}
	
		void Update(){
			float move = Input.GetAxis ("Horizontal");
			//check whether the user moves the character or not
			if (move == 0) {
				skeletonAnimation.AnimationName = Constants.ANIM_IDLE;
				return;
			} else {
				rb2d.velocity = new Vector2 (move * maxSpeed, rb2d.velocity.y);
				if (move > 0 && !facingRight)
					Flip ();
				else if (move < 0 && facingRight)
					Flip ();
				skeletonAnimation.AnimationName = Constants.ANIM_WALK;
			}
		}

		void Flip()
		{
			facingRight = !facingRight;
			Vector3 theScale = transform.localScale;
			theScale.x *= -1;
			transform.localScale = theScale;
		}


	}
}
