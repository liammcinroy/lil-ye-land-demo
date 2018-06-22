using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MobileNoReleaseButtons : Button {

    public List<Button> childButtons;

	// Use this for initialization
	void Start () {
        for (int i = 0; i < this.transform.childCount; ++i)
            if (this.transform.GetChild(i).gameObject.GetComponent<Button>() != null)
                childButtons.Add(this.transform.GetChild(i).gameObject.GetComponent<Button>());
	}
	
	// Update is called once per frame
	void Update () {

        foreach (Button button in childButtons)
            button.interactable = this.IsPressed();
    }
}
