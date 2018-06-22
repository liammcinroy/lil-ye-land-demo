using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour {

    [HideInInspector]
    public Animator animator;
    [HideInInspector]
    public new Rigidbody2D rigidbody2D;

    public AudioClip jumpSound;

    public bool default_left_move = true;

    // Animation variables set by inherited classes
    [HideInInspector]
    public bool moving = false;
    [HideInInspector]
    public bool facing_left = false;

    // Motion variables set by inherited classes
    [HideInInspector]
    public bool jump = false;
    [HideInInspector]
    public float velocity_direction = 0;

    // Constants set in inspector
    public float jump_force_magnitude;
    public float move_force_magnitude;
    public float max_velocity;

    // Variables for motion shared by all characters
    private bool on_floor = false;

    public Vector2 getXY(Vector3 vec)
    {
        return new Vector2(vec.x, vec.y);
    }

    public virtual void get_animator_params()
    {
    }

    public virtual void get_motion_params()
    {
    }

    // Use this for initialization
    public void Start() {
        animator = this.GetComponent<Animator>();
        rigidbody2D = this.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    public void Update() {
        // motion, call before animator for jump
        get_motion_params();
        if (rigidbody2D != null)
        {
            Vector2 parent_vel = Vector2.zero;
            if (this.transform.parent != null)
                parent_vel = this.transform.parent.GetComponent<Rigidbody2D>().velocity;

            Vector2 vel = rigidbody2D.velocity;

            if (jump && on_floor)
            {
                vel.y = jump_force_magnitude;
                animator.SetBool("jumping", true);
                animator.SetBool("landing", false);
                this.GetComponent<AudioSource>().PlayOneShot(jumpSound);
            }
            else
                animator.SetBool("jumping", false);

            vel.x = Mathf.Clamp(parent_vel.x + velocity_direction * max_velocity * (on_floor ? 1 : .8f), -max_velocity, max_velocity);

            rigidbody2D.velocity = vel;
        }

        // get animation
        get_animator_params();
        animator.SetBool("moving", moving);

        if (rigidbody2D != null)
        {
            if (rigidbody2D.velocity.y < -.01f)
            {
                RaycastHit2D rayhit = Physics2D.Raycast(getXY(this.transform.position), Vector2.down);
                if (rayhit.collider != null && rayhit.distance < 10)
                {
                    animator.SetBool("landing", true);
                    animator.SetBool("jumping", false);
                }
            }
            animator.SetFloat("y-vel", rigidbody2D.velocity.y);
        }

        // mirror if necessary
        Vector3 scale = this.transform.localScale;
        scale.x = (facing_left ^ default_left_move ? -1 : 1) * Mathf.Abs(scale.x);
        this.transform.localScale = scale;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        RaycastHit2D hit = Physics2D.Raycast(getXY(this.transform.position), Vector2.down, this.GetComponent<BoxCollider2D>().size.y / 2);
        if (collision.gameObject.tag == "Floor" && hit.collider != null)
            on_floor = true;
    }

    public void OnCollisionStay2D(Collision2D collision)
    {
        RaycastHit2D hit = Physics2D.Raycast(getXY(this.transform.position), Vector2.down, this.GetComponent<BoxCollider2D>().size.y / 2);
        if (collision.gameObject.tag == "Floor" && hit.collider != null)
            on_floor = true;

        if (Mathf.Abs(velocity_direction) > 0.01f && Mathf.Abs(rigidbody2D.velocity.x) < .01f)
        {
            hit = Physics2D.Raycast(getXY(this.transform.position), velocity_direction * Vector2.right, .3f);
            if (hit.collider == null)
                transform.position = Vector3.MoveTowards(transform.position, Vector3.right * velocity_direction, Time.deltaTime * max_velocity) + Vector3.up * .2f;
        }
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Floor")
            on_floor = false;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
    }
}
