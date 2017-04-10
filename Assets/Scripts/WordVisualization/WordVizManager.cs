using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using opal;


public class WordVizManager: MonoBehaviour 
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
	private float origY;
    [HideInInspector]
    public string letterHolderValue;
    [HideInInspector]
    public int letterOrder;
    [HideInInspector]
    public bool selected = false;

	public Camera cam;

	Text textBox;
	Dictionary<string,GameObject> LetterTiles;


	void Start()
    {
		if (cam == null) {
			cam = Camera.main;
		}
		//create a new tag called for letter holder / letter tile objects
		TagHelper.AddTag(Constants.WORDVIZ_LETTERHOLDER);
		TagHelper.AddTag(Constants.WORDVIZ_LETTERTILE);
    }

	public void destoryWords ()
	{
		foreach (Transform childTransform in parentObj.transform) Destroy(childTransform.gameObject);
		Destroy (parentObj);
	}
		

	public void LoadGame(string iword)
    {
		GameObject[] tmp = GameObject.FindGameObjectsWithTag (Constants.WORDVIZ_LETTERHOLDER);
		GameObject[] tmp2 = GameObject.FindGameObjectsWithTag (Constants.WORDVIZ_LETTERHOLDER);
		opal.Logger.Log ("array length: "+tmp.Length);
		if (tmp.Length != 0 || tmp2.Length != 0) {
			opal.Logger.LogError ("Either letter holders or letter tiles already exist");
			return;
		} else if (tmp.Length != 0 && tmp2.Length != 0) {
			return;
		}
		
		
        GameObject tempObj;
		GameObject tempObj_lettertile;
        Vector2 pos;

        // define center position
        center = cam.ScreenToWorldPoint(new Vector3(Screen.width/2, Screen.height/2, 40));
        
		opal.Logger.Log ("transform position: "+transform.position);
        // Instantiate parent object for other game objects
		parentObj = Instantiate(parentObject, center,  Quaternion.identity) as GameObject;


		//parentObj.transform.rotation = Quaternion.AngleAxis (270,Vector3.right);
		opal.Logger.Log ("cam screen to world point width + height: "+center);
		word = iword;
        // ...and split to the array
        _word = word.ToCharArray();
        // spacing between tiles can never be below 1
        spacing = Mathf.Clamp(spacing, 1, float.MaxValue);

	
		float Zoffset = 40.0f;

        // center field elements as much as possible
        bool left = false;
        int leftv = 0, rightv = 1;
        Dictionary<GameObject, float> fields = new Dictionary<GameObject, float>();

        for (int i = 0; i < _word.Length; i++)
        {
            left = !left;
            if (left)
            {
				Vector3 tmp_pos=cam.ScreenToWorldPoint(new Vector3(((Screen.width / 2)) - (leftv * 38 * letterHolder.transform.localScale.x * spacing), Screen.height/2, Zoffset));
				Vector3 tmp_pos2 = new Vector3(tmp_pos.x,tmp_pos.y +5, tmp_pos.z);
				origY = tmp_pos2.y;
				tempObj = Instantiate(letterHolder,tmp_pos , Quaternion.AngleAxis(90,Vector3.left)) as GameObject;
				tempObj_lettertile = Instantiate(letterTile, tmp_pos2, Quaternion.identity) as GameObject;
				leftv++;
            }
            else
            {
				Vector3 tmp_pos = cam.ScreenToWorldPoint(new Vector3(((Screen.width / 2)) + (rightv * 38 * letterHolder.transform.localScale.x * spacing), Screen.height/2, Zoffset));
				Vector3 tmp_pos2 = new Vector3(tmp_pos.x,tmp_pos.y +5, tmp_pos.z);
				tempObj = Instantiate(letterHolder, tmp_pos, Quaternion.AngleAxis(90,Vector3.left)) as GameObject;
				tempObj_lettertile = Instantiate(letterTile, tmp_pos2, Quaternion.identity) as GameObject;
				rightv++;
            }
			tempObj.transform.localScale = new Vector3(0.15F, 0.15f, 0.15f);
            fields.Add(tempObj, tempObj.transform.position.x);

			tempObj_lettertile.transform.eulerAngles = new Vector3(270.0f, 0, 0);
			tempObj_lettertile.transform.localScale = new Vector3(0.1F, 1.0f, 0.1f);
			tempObj_lettertile.GetComponent<LetterValue>().SetValue(_word[i].ToString());
			tempObj_lettertile.transform.parent = parentObj.transform;
			tempObj_lettertile.name = tileName+ i.ToString () + "_" + _word [i].ToString ();
        }

        // sort field elements
        fields = fields.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

        leftv = 0;



        foreach (KeyValuePair<GameObject, float> targ in fields)
        {
            targ.Key.GetComponent<HolderValue>().SetOrder(leftv);
            targ.Key.name = holderName + leftv.ToString();
            targ.Key.transform.parent = parentObj.transform;
			targ.Key.tag = Constants.WORDVIZ_LETTERHOLDER;
            leftv++;
        }

        tempValues = new string[_word.Length];
	
    }

	public void onBadSyllableReceived(string syllable){
		opal.Logger.Log ("on bad syllable received...");
		int pos = word.IndexOf(syllable);
		char[] _syllable = syllable.ToCharArray ();
		// check the length of the given syllable 
		for (int i = 0; i < _syllable.Length; i++)
		{
			int keyIndex = i + pos;
			char ichar = _syllable [i];

			current = GameObject.Find (tileName + keyIndex + "_" + ichar.ToString ());
			if (current == null) {
				Debug.LogError ("no object is found for : " + ichar);
			}
			//current.transform.eulerAngles = new Vector3 (0, 0, current.transform.position.z);
			tempValues [keyIndex] = current.GetComponent<LetterValue> ().GetValue ();
			//Debug.Log ("key index: "+keyIndex);
			GameObject iholder = GameObject.Find (holderName + keyIndex.ToString ());

			//GameObject.Find (holderName + keyIndex.ToString ()).transform.position.x, current.transform.position.y, 
			//GameObject.Find ("holderName" + keyIndex.ToString ()).transform.position.z
			if (current.transform.position == iholder.transform.position) {
				opal.Logger.Log ("reposition ...");
				opal.Logger.Log ("old y: " + current.transform.position.y.ToString ());
				//float newY = origY;
				//current.transform.localPosition.y = origY;
				//current.transform.position.Set (current.transform.position.x,newY,current.transform.position.z);
				selected = false;
			}

		}


	}

	public void onSyllableReceived(string syllable)
    {
		opal.Logger.Log ("on syllable received runs....");
	
		//starting position of the syllable in the word
		int pos = word.IndexOf(syllable);
		if (pos == -1)
			Debug.LogError ("cannot find the syllable in the word string: "+syllable);
		opal.Logger.Log ("pos: "+pos.ToString());
		char[] _syllable = syllable.ToCharArray ();
		// check the length of the given syllable 
		for (int i = 0; i < _syllable.Length; i++)
		{
			int keyIndex = i + pos;
			char ichar = _syllable [i];

			current = GameObject.Find (tileName + keyIndex + "_" + ichar.ToString ());
			if (current == null) {
				Debug.LogError ("no object is found for : " + ichar);
			}
			//current.transform.eulerAngles = new Vector3 (0, 0, current.transform.position.z);
			tempValues [keyIndex] = current.GetComponent<LetterValue> ().GetValue ();
			//Debug.Log ("key index: "+keyIndex);
			GameObject iholder = GameObject.Find (holderName + keyIndex.ToString ());

			//GameObject.Find (holderName + keyIndex.ToString ()).transform.position.x, current.transform.position.y, 
			//GameObject.Find ("holderName" + keyIndex.ToString ()).transform.position.z
			current.transform.position = iholder.transform.position;
			selected = false;
		}

        // if left button is clicked and there's no selected letter
        if (Input.GetMouseButtonDown(0) && !selected)
        {
			//Debug.Log ("mouse button down...");
            container = false;
            letterOrder = -1;
            letterHolderValue = "";
            // check if we have clicked the letter...
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
				
                // ...and select it
                if (hit.transform.gameObject.name.Contains(tileName))
                {
					
                    current = hit.transform.gameObject;
                    current.layer = 2;
                    selected = true;
                }
            }
        }

        // selected letter draging
        if (Input.GetMouseButton(0) && selected)
        {
			
            current.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, current.transform.position.z));
        }

        // on left mouse release
        if (Input.GetMouseButtonUp(0) && selected)
        {

            current.layer = 0;
            // if we are not over field, drop letter
            if (letterOrder == -1)
            {
                selected = false;
                return;
            } // else place letter in right fieled
            else
            {
				//Debug.LogError ("letter order: "+letterOrder.ToString());
                current.transform.eulerAngles = new Vector3(0, 0, 0);
                tempValues[letterOrder] = current.GetComponent<LetterValue>().GetValue();
                current.transform.position = new Vector3(GameObject.Find(holderName + letterOrder.ToString()).transform.position.x, current.transform.position.y, GameObject.Find("holderName" + letterOrder.ToString()).transform.position.z);
                selected = false;
            }
        }
    }
		
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
