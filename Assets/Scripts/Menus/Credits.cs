using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Credits : MonoBehaviour {

    public float time;

    private RectTransform rectTransform;

    private Vector2 startPos;
    public GameObject stopper;
    private float increaseY;

	// Use this for initialization
	void Start () {
        rectTransform = this.GetComponent<RectTransform>();
        startPos = rectTransform.position;
        increaseY = Mathf.Abs(stopper.GetComponent<RectTransform>().position.y);

        Debug.Log(increaseY);
        GameObject.Find("return").GetComponent<Button>().onClick.AddListener(delegate { Application.LoadLevel("main-menu"); });
        if (GameObject.Find("campaign") != null)
            GameObject.Find("campaign").GetComponent<Button>().onClick.AddListener(delegate { Application.OpenURL("https://www.kickstarter.com/projects/1538450318/1618577795"); });

//#if UNITY_EDITOR
//        Time.timeScale = 5;
//#endif
    }

    // Update is called once per frame
    void Update()
    {
        rectTransform.position = Vector2.Lerp(startPos, startPos + increaseY * Vector2.up, Time.timeSinceLevelLoad / time);
    }
}
