using UnityEngine;
using System.Collections;

public class HolderValue : MonoBehaviour {
    public int order;

	void Start(){
		GetComponent<MeshRenderer>().sortingLayerName="Layer Words";
	}

    public void SetOrder(int value)
    {
        order = value;
    }

    public int GetOrder()
    {
        return order;
    }

    void OnMouseExit()
    {
        //WordVizManager.Instance.letterOrder = -1;
    }

    void OnMouseOver()
    {
        //WordVizManager.Instance.letterOrder = order;
    }
}
