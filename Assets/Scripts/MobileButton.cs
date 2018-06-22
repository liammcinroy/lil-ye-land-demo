using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MobileButton : Button {

    [HideInInspector]
    public bool pressed = false;
    [HideInInspector]
    public bool downFrame = false;

	// Update is called once per frame
	void Update () {
        if (pressed && downFrame)
            downFrame = false;
        if (!pressed && IsPressed())
            downFrame = true;
        pressed = IsPressed();
	}
}
