using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobBehavior : MonoBehaviour {

    public float degree;

    private float velocity = 1;

    private float startTime;
    private float flip;
    private bool stop = false;

	// Use this for initialization
	void Awake () {
        startTime = Time.time;
        flip = -GameObject.Find("boss").GetComponent<BossBehaviorV2>().flip;
    }
	
	// Update is called once per frame
	void Update () {
        if (Time.time - startTime > 2f)
            Destroy(this.gameObject);

        Vector3 direction = new Vector3(flip * Mathf.Cos(degree), Mathf.Sin(degree), 0);

        if (!stop)
            this.transform.position += velocity * direction;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" || (collision.tag == "Floor" && !collision.name.Contains("platform")))
        {
            stop = true;
            this.GetComponent<Animator>().SetBool("explode", true);
            Destroy(this.gameObject, .4f);
        }
    }
}
