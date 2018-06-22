using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoIntermittentLight : MonoBehaviour {

    public float on_time;
    public float off_time;

    public bool on;

    public GameObject player;

    public bool turn_off_torch_on;

    private float last_switch_time;

    private Color torch_color;


    // Use this for initialization
    void Start () {
        last_switch_time = Time.time;
        torch_color = Color.white;
        torch_color.a = .5f;
    }
	
	// Update is called once per frame
	void Update () {
        if (player.GetComponent<DemoYe>().torch_on && turn_off_torch_on)
            on = false;

        if (on)
        {
            this.GetComponent<SpriteRenderer>().color = torch_color;
            if (Time.time - last_switch_time > on_time)
            {
                last_switch_time = Time.time;
                on = false;
            }
        }

        else
        {
            this.GetComponent<SpriteRenderer>().color = Color.clear;
            if (Time.time - last_switch_time > off_time)
            {
                last_switch_time = Time.time;
                on = true;
            }
        }
	}
}
