using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using opal;

namespace AssemblyCSharp
{
	public class WordVizHandler
	{

		public bool rotateTiles = false;                // if true, tiles with letters will be rotated. This can make game harder.
		public GameObject letterTile;
		public GameObject letterHolder;
		public GameObject parentObject;
		public float spacing;                           // Spacing between the letter fields

		private int hintPos;
		private Ray ray;
		private RaycastHit hit;
		private GameObject current, backObj, parentObj;
		private char[] _word;
		private Vector3 center;
		private string[] tempValues;
		[HideInInspector]
		public bool container = false;
		private string tileName = "tileName";
		private string holderName = "holderName";
		private string word;
		[HideInInspector]
		public string letterHolderValue;
		[HideInInspector]
		public int letterOrder;
		[HideInInspector]
		public bool selected = false;

		public Camera cam;

		Text textBox;
		Dictionary<string,GameObject> LetterTiles;


		public WordVizHandler ()
		{
			
		}


			



//		public void LoadGame(string iword)
//		{
//
//
//				GameObject tempObj;
//				Vector2 pos;
//
//				// define center position
//				center = cam.ScreenToWorldPoint(new Vector3(Screen.width/2, Screen.height/2, cam.nearClipPlane));
//
//				// Instantiate parent object for other game objects
//				parentObj = Instantiate(parentObject, transform.position, Quaternion.identity) as GameObject;
//
//				word = iword;
//				// ...and split to the array
//				_word = word.ToCharArray();
//				// spacing between tiles can never be below 1
//				spacing = Mathf.Clamp(spacing, 1, float.MaxValue);
//
//				// center field elements as much as possible
//				bool left = false;
//				int leftv = 0, rightv = 1;
//				Dictionary<GameObject, float> fields = new Dictionary<GameObject, float>();
//				for (int i = 0; i < _word.Length; i++)
//				{
//					left = !left;
//					if (left)
//					{
//						tempObj = Instantiate(letterHolder, new Vector3(((Screen.width / 2)) - (leftv * 38 * letterHolder.transform.localScale.x * spacing), center.y, 200), Quaternion.identity) as GameObject;
//						tempObj.SetActive (true);
//						leftv++;
//					}
//					else
//					{
//						tempObj = Instantiate(letterHolder, new Vector3(((Screen.width / 2)) + (rightv * 38 * letterHolder.transform.localScale.x * spacing), center.y, 200), Quaternion.identity) as GameObject;
//						rightv++;
//					}
//					fields.Add(tempObj, tempObj.transform.position.x);
//				}
//
//				// sort field elements
//				fields = fields.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
//
//				leftv = 0;
//				foreach (KeyValuePair<GameObject, float> targ in fields)
//				{
//					targ.Key.GetComponent<HolderValue>().SetOrder(leftv);
//					targ.Key.name = holderName + leftv.ToString();
//					targ.Key.transform.parent = parentObj.transform;
//					Vector3 p = cam.WorldToScreenPoint(targ.Key.transform.position);
//					leftv++;
//				}
//
//				tempValues = new string[_word.Length];
//
//				// finnaly, instantiate letters
//				for (int i = 0; i < _word.Length; i++)
//				{
//					pos = new Vector2(UnityEngine.Random.Range(100, Screen.width - 100), UnityEngine.Random.Range(Screen.height - 50, Screen.height - 200));
//					tempObj = Instantiate(letterTile, Camera.main.ScreenToWorldPoint(new Vector3(pos.x, pos.y, 10)), Quaternion.identity) as GameObject;
//					tempObj.GetComponent<LetterValue>().SetValue(_word[i].ToString());
//
//					tempObj.transform.parent = parentObj.transform;
//
//					tempObj.name = tileName+ i.ToString () + "_" + _word [i].ToString ();
//					if (rotateTiles)
//					{
//						tempObj.transform.eulerAngles = new Vector3(0, UnityEngine.Random.Range(0, 360), 0);
//					}
//
//					//LetterTiles.Add (_word[i].ToString());
//				}
//			}

			//    void Update()
			//    {
			//
			//		string syllable = textBox.text;
			//		//starting position of the syllable in the word
			//		int pos = word.IndexOf(syllable);
			//		if (pos == -1)
			//			Debug.LogError ("cannot find the syllable in the word string: "+syllable);
			//		Debug.LogError ("pos: "+pos.ToString());
			//		char[] _syllable = syllable.ToCharArray ();
			//		// check the length of the given syllable 
			//		for (int i = 0; i < _syllable.Length; i++)
			//		{
			//			int keyIndex = i + pos;
			//			char ichar = _syllable [i];
			//
			//			current = GameObject.Find (tileName + keyIndex + "_" + ichar.ToString ());
			//			if (current == null) {
			//				Debug.LogError ("no object is found for : " + ichar);
			//			}
			//			current.transform.eulerAngles = new Vector3 (0, 0, 0);
			//			tempValues [keyIndex] = current.GetComponent<LetterValue> ().GetValue ();
			//			//Debug.Log ("key index: "+keyIndex);
			//			current.transform.position = new Vector3 (GameObject.Find (holderName + keyIndex.ToString ()).transform.position.x, current.transform.position.y, 
			//				GameObject.Find ("holderName" + keyIndex.ToString ()).transform.position.z);
			//			selected = false;
			//		}
			//
			//        // if left button is clicked and there's no selected letter
			//        if (Input.GetMouseButtonDown(0) && !selected)
			//        {
			//			//Debug.Log ("mouse button down...");
			//            container = false;
			//            letterOrder = -1;
			//            letterHolderValue = "";
			//            // check if we have clicked the letter...
			//            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			//            if (Physics.Raycast(ray, out hit))
			//            {
			//				
			//                // ...and select it
			//                if (hit.transform.gameObject.name.Contains(tileName))
			//                {
			//					
			//                    current = hit.transform.gameObject;
			//                    current.layer = 2;
			//                    selected = true;
			//                }
			//            }
			//        }
			//
			//        // selected letter draging
			//        if (Input.GetMouseButton(0) && selected)
			//        {
			//			
			//            current.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, current.transform.position.z));
			//        }
			//
			//        // on left mouse release
			//        if (Input.GetMouseButtonUp(0) && selected)
			//        {
			//
			//            current.layer = 0;
			//            // if we are not over field, drop letter
			//            if (letterOrder == -1)
			//            {
			//                selected = false;
			//                return;
			//            } // else place letter in right fieled
			//            else
			//            {
			//				//Debug.LogError ("letter order: "+letterOrder.ToString());
			//                current.transform.eulerAngles = new Vector3(0, 0, 0);
			//                tempValues[letterOrder] = current.GetComponent<LetterValue>().GetValue();
			//                current.transform.position = new Vector3(GameObject.Find(holderName + letterOrder.ToString()).transform.position.x, current.transform.position.y, GameObject.Find("holderName" + letterOrder.ToString()).transform.position.z);
			//                selected = false;
			//            }
			//        }
			//    }

			// check if our word is correct
			private bool Check()
			{
				bool result = true;
				string answer = "";
				for (int j = 0; j < tempValues.Length; j++)
				{
					answer += tempValues[j];
				}

				if (answer.ToLower() != word.ToLower())
				{
					result = false;
				}

				return result;
			}

			//    // show short message
			//    private IEnumerator ShowMsg(string msg)
			//    {
			//        message = msg;
			//        showMsg = true;
			//        yield return new WaitForSeconds(5);
			//        showMsg = false;
			//        message = "";
			//    }





	}	
}

