using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebGLCredits : MonoBehaviour
{

    public float speed;

    private RectTransform rectTransform;

    // Use this for initialization
    void Start()
    {
        rectTransform = this.GetComponent<RectTransform>();
        GameObject.Find("campaign").GetComponent<Button>().onClick.AddListener(delegate { Application.OpenURL("https://www.kickstarter.com/projects/1538450318/1618577795"); });
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 pos = rectTransform.position;

        if (pos.y < 900)
        {
            pos.y += speed * Time.deltaTime;
            rectTransform.position = pos;
        }
    }
}
