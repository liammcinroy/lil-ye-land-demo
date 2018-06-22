using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBehavior : MonoBehaviour {

    private enum Behaviors
    {
        Realign,
        Scream,
        Charge
    }

    public int maxHealth;
    [HideInInspector]
    public int health;

    public float maxFloatVel;
    public GameObject minionGhost;

    public GameObject glob;

    public GameObject player;

    public AudioClip bossScream;
    public AudioClip bossGlob;
    public AudioClip deadSound;

    private AudioSource audioSource;

    private Behaviors currentBehavior = Behaviors.Charge;
    private Behaviors lastSpecial = Behaviors.Scream;
    private bool previousFinished = true;
    private bool idled = false;

    [HideInInspector]
    public int flip = 1;
    private float chargeVel = 40 / .66f;

    private int generate_ghosts = 1;
    private bool dead = false;

    [HideInInspector]
    public bool player_entered = false;

    protected Animator animator;
    new protected Rigidbody2D rigidbody2D;

    public Color glow_color;
    private bool glow = true;
    private float last_color_change;

	// Use this for initialization
	void Start () {
        health = maxHealth;
        animator = this.GetComponent<Animator>();
        rigidbody2D = this.GetComponent<Rigidbody2D>();
        audioSource = this.GetComponent<AudioSource>();
    }
	
	// Update is called once per frame
	void Update ()
    {
        Vector3 scale = this.transform.localScale;
        if (Mathf.Sign(scale.x) * flip < 0)
            scale.x *= -flip;

        if (player_entered && previousFinished)
        {
            this.rigidbody2D.velocity = Vector2.zero;
            previousFinished = false;
            if (currentBehavior == Behaviors.Realign)
            {
                if (!idled)
                    StartCoroutine(Idle());
                else
                {
                    currentBehavior = lastSpecial == Behaviors.Scream ? Behaviors.Charge : Behaviors.Scream;
                    lastSpecial = currentBehavior;
                    switch (currentBehavior)
                    {
                        case Behaviors.Charge:
                            StartCoroutine(Charge());
                            scale.x *= -1;
                            break;
                        case Behaviors.Scream:
                            generate_ghosts++;
                            StartCoroutine(Scream());
                            break;
                    }
                    idled = false;
                }
            }
            else
                currentBehavior = Behaviors.Realign;
        }

        else if (!previousFinished && !idled && health > 0)
        {
            if (currentBehavior == Behaviors.Realign)
            {
                if (Mathf.Abs(player.transform.position.y - this.transform.position.y) < 5)
                    previousFinished = true;
                else
                    this.rigidbody2D.velocity = maxFloatVel * Mathf.Sign(player.transform.position.y - this.transform.position.y) * Vector2.up;
            }

            else if (currentBehavior == Behaviors.Charge)
            {
                this.rigidbody2D.velocity = flip * Vector2.right * chargeVel;
                if (this.rigidbody2D.position.x > 1230 || this.rigidbody2D.position.x < 1180)
                    previousFinished = true;
            }

            else
                scale.x = Mathf.Abs(scale.x) * -Mathf.Sign(player.transform.position.x - this.transform.position.x);
        }

        if (Time.timeSinceLevelLoad > 3 && !player_entered)
            player_entered = true;

        if (Time.time - last_color_change > health && health < 3 && !glow)
        {
            this.GetComponent<SpriteRenderer>().color = glow_color;
            glow = true;
            last_color_change = Time.time;
        }
        else if (Time.time - last_color_change > .5f && glow)
        {
            this.GetComponent<SpriteRenderer>().color = Color.white;
            glow = false;
            last_color_change = Time.time;
        }

        scale.x = health > 0 ? (((float) health) / maxHealth / 2  + .5f) * Mathf.Sign(scale.x) : 1;
        scale.y = health > 0 ? ((float) health) / maxHealth / 2 + .5f : 1;


        if (health <= 0 && !dead)
        {
            dead = true;
            animator.SetBool("dead", true);
            this.audioSource.PlayOneShot(deadSound);
            Instantiate(glob, this.transform.position, new Quaternion(0, 0, 0, 0)).GetComponent<GlobBehavior>().degree = 0;
            Instantiate(glob, this.transform.position, new Quaternion(0, 0, 0, 0)).GetComponent<GlobBehavior>().degree = Mathf.PI / 4;
            Instantiate(glob, this.transform.position, new Quaternion(0, 0, 0, 0)).GetComponent<GlobBehavior>().degree = -Mathf.PI / 4;
            Instantiate(glob, this.transform.position, new Quaternion(0, 0, 0, 0)).GetComponent<GlobBehavior>().degree = Mathf.PI / 2;
            Instantiate(glob, this.transform.position, new Quaternion(0, 0, 0, 0)).GetComponent<GlobBehavior>().degree = -Mathf.PI / 2;
            Instantiate(glob, this.transform.position, new Quaternion(0, 0, 0, 0)).GetComponent<GlobBehavior>().degree = 3 * Mathf.PI / 4;
            Instantiate(glob, this.transform.position, new Quaternion(0, 0, 0, 0)).GetComponent<GlobBehavior>().degree = -3 * Mathf.PI / 4;
            player.GetComponent<PlayableCharacter>().Invoke("FinishLevel", 1);
        }

        this.transform.localScale = scale;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if (collision.tag == "TorchLight" && Time.time - lastHitTime > 1f && player_entered)
        //{
        //    StartCoroutine(Hurt());
        //    health--;
        //}
    }

    IEnumerator Charge()
    {
        animator.SetBool("charge", true);
        yield return new WaitForSeconds(0.66f);
        animator.SetBool("charge", false);
        flip *= -1;
        previousFinished = true;
        this.rigidbody2D.velocity = Vector2.zero;
    }

    IEnumerator Scream()
    {
        int numGen = 2; // Random.Range(1, 3);
        this.health--;
        animator.SetBool("scream", true);
        audioSource.PlayOneShot(generate_ghosts % 2 == 1 ? bossScream : bossGlob);
        yield return new WaitForSeconds(.25f);
        for (int i = 0; i < numGen && generate_ghosts % 2 == 1 && health > 0; ++i)
        {
            yield return new WaitForSeconds(0.35f);
            GameObject ghost = Instantiate(minionGhost, this.transform.position, new Quaternion(0, 0, 0, 0));
            ghost.GetComponent<GhostBehaviors>().player = player;
        }
        if (generate_ghosts % 2 != 1 && health > 0)
        {
            Instantiate(glob, this.transform, false).GetComponent<GlobBehavior>().degree = 0;
            Instantiate(glob, this.transform, false).GetComponent<GlobBehavior>().degree = Mathf.PI / 4;
            Instantiate(glob, this.transform, false).GetComponent<GlobBehavior>().degree = -Mathf.PI / 4;
            yield return new WaitForSeconds(.5f);
        }
        animator.SetBool("scream", false);
        previousFinished = true;
    }

    IEnumerator Hurt()
    {
        animator.SetBool("hurt", true);
        yield return new WaitForSeconds(0.75f);
        animator.SetBool("hurt", false);
    }

    IEnumerator Idle()
    {
        idled = true;
        yield return new WaitForSeconds(Random.Range(.5f, 1f));
        previousFinished = true;
    }
}
