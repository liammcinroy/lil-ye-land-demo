using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour {

    public bool move_x;
    public float radius;
    public float speed;

    private Vector2 target_position;
    private float flip;
    new private Rigidbody2D rigidbody2D;

	// Use this for initialization
	void Start () {
        rigidbody2D = this.GetComponent<Rigidbody2D>();
        target_position = this.rigidbody2D.position;

        if (move_x)
            target_position.x += radius;
        else
            target_position.y += radius;
        flip = Mathf.Sign(radius);
    }
	
	// Update is called once per frame
	void Update () {
        if (move_x && (target_position.x - this.transform.position.x) * flip < 0.1f)
        {
            flip *= -1;
            target_position.x += flip * 2 * Mathf.Abs(radius);
        }

        else if (!move_x && (target_position.y - this.transform.position.y) * flip < 0.1f)
        {
            flip *= -1;
            target_position.y += flip * 2 * Mathf.Abs(radius);
        }

        //rigidbody2D.MovePosition(Vector2.MoveTowards(this.rigidbody2D.position, target_position, Time.deltaTime * speed));

        Vector2 vel = Vector2.zero;
        if (move_x)
            vel.x = speed * flip;
        else
            vel.y = speed * flip;
        rigidbody2D.velocity = vel;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ContactPoint2D[] contacts = new ContactPoint2D[1];
        collision.GetContacts(contacts);
        if (collision.transform.tag == "Player" && contacts[0].point.y < collision.transform.position.y)
            collision.transform.parent = this.transform;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.transform.tag == "Player")
            collision.transform.parent = null;
    }

    //private void OnCollisionStay2D(Collision2D collision)
    //{
    //    if (collision.gameObject.tag == "Player" && move_x)
    //        collision.gameObject.GetComponent<Rigidbody2D>().velocity += this.rigidbody2D.velocity;
    //}
}
