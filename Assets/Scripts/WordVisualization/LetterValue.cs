using UnityEngine;
using System.Collections;

public class LetterValue : MonoBehaviour {
    public GameObject textObj; 
    [HideInInspector]
    public string textValue = "";

    private Ray ray;
    private RaycastHit hit;

    public void SetValue(string value)
    {
        textObj.GetComponent<TextMesh>().text = value;
        textValue = value;
    }

    public string GetValue()
    {
        return textValue;
    }
}
