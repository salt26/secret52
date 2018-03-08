using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour {

    private bool isOn;

	// Use this for initialization
	void Start () {
        isOn = true;
	}
	
    public bool GetIsOn()
    {
        return isOn;
    }

    public void Check()
    {
        if (!isOn) return;
        isOn = false;
        gameObject.SetActive(false);
    }
}
