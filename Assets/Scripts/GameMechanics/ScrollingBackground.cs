using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace opal{
	public class ScrollingBackground : MonoBehaviour {

		public bool scrolling, paralax;
		public float backgroundSize=12f;
		public float paralaxSpeed;
		private Transform cameraTransform;
		private Transform[] layers;
	
		private float viewZone = 10;
		private int leftIndex;
		private int rightIndex;
		private float lastCameraX;


		private void Start(){
			scrolling = true;
			cameraTransform = Camera.main.transform;
			lastCameraX = cameraTransform.position.x;

			//get background layers
			layers = new Transform[transform.childCount];
			Logger.Log ("child: "+transform.childCount);
			for (int i = 0; i < transform.childCount; i++) {
				layers [i] = transform.GetChild (i);
				//layers [i].position = new Vector3 (i*backgroundSize -12f, -2.0f, 0.0f);
			}
			leftIndex = 0;
			rightIndex = layers.Length - 1;
		}

		private void Update(){
			if (paralax) {
				float deltaX = cameraTransform.position.x - lastCameraX;
				transform.position += Vector3.right * (deltaX * paralaxSpeed);
			}
			lastCameraX = cameraTransform.position.x;
			if (scrolling) {
				if (cameraTransform.position.x < (layers [leftIndex].transform.position.x + viewZone))
					ScrollLeft ();
				if (cameraTransform.position.x > (layers [rightIndex].transform.position.x))
					ScrollRight ();
				
			}
		}

		private void ScrollLeft()
		{
			
			int lastRight = rightIndex;
			layers [rightIndex].position = new Vector3(layers [leftIndex].position.x - backgroundSize,layers [rightIndex].position.y,layers [rightIndex].position.z);
			leftIndex = rightIndex;
			rightIndex--;
			if (rightIndex < 0)
				rightIndex = layers.Length - 1;
		}

		private void ScrollRight()
		{
			
			int lastLeft = leftIndex;
			layers [leftIndex].position = new Vector3(layers [rightIndex].position.x + backgroundSize,layers [rightIndex].position.y,layers [rightIndex].position.z);
			rightIndex = leftIndex;
			leftIndex++; 
			if (leftIndex == layers.Length)
				leftIndex = 0;
		}
	}
}
