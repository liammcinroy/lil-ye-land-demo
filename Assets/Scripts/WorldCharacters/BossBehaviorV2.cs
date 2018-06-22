using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBehaviorV2 : MonoBehaviour {

    [HideInInspector]
    public enum Behaviors
    {
        Realign,
        ScreamLeft,
        ScreamRight,
        Ghost,
        ChargeTopRight,
        ChargeTopLeft,
        ChargeBottomRight,
        ChargeBottomLeft,
        Die
    }

    private Dictionary<Behaviors, Vector2> behaviorStartLocations = new Dictionary<Behaviors, Vector2>() {
        { Behaviors.Ghost, new Vector2(1200, 70) },
        { Behaviors.ScreamLeft, new Vector2(1185, 51) },
        { Behaviors.ScreamRight, new Vector2(1215, 51) },
        { Behaviors.ChargeBottomLeft, new Vector2(1170, 51) },
        { Behaviors.ChargeBottomRight, new Vector2(1230, 51) },
        { Behaviors.ChargeTopLeft, new Vector2(1170, 60) },
        { Behaviors.ChargeTopRight, new Vector2(1230, 60) },
        { Behaviors.Die, new Vector2(1200, 70) },
    };

    private const float chargeVel = 40;

    private Dictionary<Behaviors, string> behaviorsAnimations;

    private Dictionary<Behaviors, Vector2> behaviorVelocities = new Dictionary<Behaviors, Vector2>() {
        { Behaviors.Ghost, new Vector2(0, 0) },
        { Behaviors.ScreamLeft, new Vector2(0, 0) },
        { Behaviors.ScreamRight, new Vector2(0, 0) },
        { Behaviors.ChargeBottomLeft, new Vector2(chargeVel, 0) },
        { Behaviors.ChargeBottomRight, new Vector2(-chargeVel, 0) },
        { Behaviors.ChargeTopLeft, new Vector2(chargeVel, 0) },
        { Behaviors.ChargeTopRight, new Vector2(-chargeVel, 0) },
        { Behaviors.Die, new Vector2(0, 0) }
    };

    private Dictionary<Behaviors, int> behaviorScale = new Dictionary<Behaviors, int>() {
        { Behaviors.Realign, 1 },
        { Behaviors.Ghost, 1 },
        { Behaviors.ScreamLeft, -1 },
        { Behaviors.ScreamRight, 1 },
        { Behaviors.ChargeBottomLeft, 1 },
        { Behaviors.ChargeBottomRight, -1 },
        { Behaviors.ChargeTopLeft, 1 },
        { Behaviors.ChargeTopRight, -1 },
        { Behaviors.Die, 1 }
    };

    public float maxFloatVel;
    public GameObject minionGhost;

    public GameObject glob;

    public GameObject player;

    public AudioClip bossScream;
    public AudioClip bossGlob;
    public AudioClip deadSound;
    public AudioClip victorySound;

    private AudioSource audioSource;

    private Behaviors[] behaviorsSequence = new Behaviors[]{
        Behaviors.Ghost,
        Behaviors.ChargeBottomRight,
        Behaviors.ScreamLeft,
        Behaviors.Ghost,
        Behaviors.ChargeBottomLeft,
        Behaviors.ScreamRight,
        Behaviors.ChargeTopRight,
        Behaviors.Die };

    private int behaviorIndex = -1;

    private Behaviors currentBehavior = Behaviors.Realign;

    private Vector2 targetStartLocation;
    private Vector2 currentBehaviorVelocity;

    [HideInInspector]
    public int flip = 1;

    private bool previousFinished = true;
    private bool hitWall = false;
    private bool idled = false;
    private bool dead = false;

    [HideInInspector]
    public bool beginSequence = false;

    protected Animator animator;
    new protected Rigidbody2D rigidbody2D;

    public Color glow_color;
    private bool glow = true;
    private float last_color_change;

    // Use this for initialization
    void Awake() {
        animator = this.GetComponent<Animator>();
        rigidbody2D = this.GetComponent<Rigidbody2D>();
        audioSource = this.GetComponent<AudioSource>();

        behaviorsAnimations = new Dictionary<Behaviors, string>() {
            { Behaviors.Ghost, "GenGhosts" },
            { Behaviors.ScreamLeft, "Scream" },
            { Behaviors.ScreamRight, "Scream" },
            { Behaviors.ChargeBottomLeft, "Charge" },
            { Behaviors.ChargeBottomRight, "Charge" },
            { Behaviors.ChargeTopLeft, "Charge" },
            { Behaviors.ChargeTopRight, "Charge" },
            { Behaviors.Die, "OnDeath" }
        };
    }

    // Update is called once per frame
    void Update()
    {
        if (beginSequence)
        {
            if (previousFinished)
            {
                this.rigidbody2D.velocity = Vector2.zero;
                previousFinished = false;
                if (idled && currentBehavior != Behaviors.Realign)
                {
                    Debug.Log(currentBehavior);
                    targetStartLocation = behaviorStartLocations[currentBehavior];
                    currentBehaviorVelocity = behaviorVelocities[currentBehavior];
                    flip = behaviorScale[currentBehavior];
                    Vector3 scale = this.transform.localScale;
                    scale.x = flip;
                    this.transform.localScale = scale;

                    if (currentBehavior == Behaviors.Die)
                    {
                        dead = true;
                        animator.SetBool("dead", true);
                        Invoke("OnDeath", 1);
                    }
                    else
                        StartCoroutine(behaviorsAnimations[currentBehavior]);
                }
                else if (currentBehavior == Behaviors.Realign && behaviorIndex + 1 < behaviorsSequence.Length)
                {
                    behaviorIndex++;
                    currentBehavior = behaviorsSequence[behaviorIndex];

                    idled = false;
                    StartCoroutine(Idle());
                }
                else if (currentBehavior != Behaviors.Realign)
                {
                    currentBehavior = Behaviors.Realign;
                    if (behaviorIndex + 1 < behaviorsSequence.Length)
                    {
                        targetStartLocation = behaviorStartLocations[behaviorsSequence[behaviorIndex + 1]];
                        flip = (int)Mathf.Sign(targetStartLocation.x - this.transform.position.x);
                        Vector3 scale = this.transform.localScale;
                        scale.x = flip;
                        this.transform.localScale = scale;
                    }

                    idled = true;
                }
                else if (!dead)
                {
                    dead = true;
                    animator.SetBool("dead", true);
                    Invoke("OnDeath", 1);
                }
            }

            else if (!previousFinished && idled)
            {
                if (currentBehavior == Behaviors.Realign)
                {
                    if (Mathf.Abs(targetStartLocation.x - this.transform.position.x) > 1)
                    {
                        this.animator.SetBool("charge", true);
                        if (this.transform.position.x < 1220 && this.transform.position.x > 1180)
                            this.rigidbody2D.velocity = chargeVel * Mathf.Sign(targetStartLocation.x - this.transform.position.x) * Vector2.right;
                        else if (!hitWall)
                            StartCoroutine(Wall(new Vector2(targetStartLocation.x, this.transform.position.y)));
                    }
                    else if (Mathf.Abs(targetStartLocation.y - this.transform.position.y) > 1)
                    {
                        this.animator.SetBool("charge", false);
                        this.rigidbody2D.velocity = maxFloatVel * Mathf.Sign(targetStartLocation.y - this.transform.position.y) * Vector2.up;
                    }
                    else
                    {
                        this.animator.SetBool("charge", false);
                        previousFinished = true;
                    }
                }

                else
                    this.rigidbody2D.velocity = currentBehaviorVelocity;
            }
        }

        else
        {
            this.rigidbody2D.velocity = new Vector2(0, -12.0f / 5);

            if (Time.timeSinceLevelLoad > 5)
                beginSequence = true;
        }

        // Color
        if (Time.time - last_color_change > .5f && behaviorsSequence.Length - behaviorIndex < 3 && !glow && !dead)
        {
            this.GetComponent<SpriteRenderer>().color = glow_color;
            glow = true;
            last_color_change = Time.time;
        }
        else if (Time.time - last_color_change > .5f && glow && !dead)
        {
            this.GetComponent<SpriteRenderer>().color = Color.white;
            glow = false;
            last_color_change = Time.time;
        }
    }

    IEnumerator Charge()
    {
        animator.SetBool("charge", true);
        yield return new WaitForSeconds(45 / chargeVel);

        if (Mathf.Sign(behaviorStartLocations[behaviorsSequence[behaviorIndex + 1]].x - this.transform.position.x) * flip < 0)
        {
            animator.SetBool("charge", false);
            currentBehaviorVelocity = Vector2.zero;
        }
        previousFinished = true;
        idled = false;
    }

    IEnumerator GenGhosts()
    {
        int numGen = 2; // Random.Range(1, 3);
        animator.SetBool("scream", true);
        audioSource.PlayOneShot(bossScream);
        for (int i = 0; i < numGen; ++i)
        {
            GameObject ghost = Instantiate(minionGhost, this.transform.position, new Quaternion(0, 0, 0, 0));
            ghost.GetComponent<GhostBehaviors>().player = player;
            yield return new WaitForSeconds(0.75f);
        }
        animator.SetBool("scream", false);
        previousFinished = true;
        idled = false;
    }

    IEnumerator Scream()
    {
        animator.SetBool("scream", true);
        audioSource.PlayOneShot(bossGlob);
        yield return new WaitForSeconds(.25f);
        Instantiate(glob, this.transform.position, new Quaternion(0, 0, 0, 0)).GetComponent<GlobBehavior>().degree = 0;
        Instantiate(glob, this.transform.position, new Quaternion(0, 0, 0, 0)).GetComponent<GlobBehavior>().degree = Mathf.PI / 4;
        Instantiate(glob, this.transform.position, new Quaternion(0, 0, 0, 0)).GetComponent<GlobBehavior>().degree = -Mathf.PI / 4;
        yield return new WaitForSeconds(.5f);
        animator.SetBool("scream", false);
        previousFinished = true;
        idled = false;
    }

    IEnumerator Hurt()
    {
        animator.SetBool("hurt", true);
        yield return new WaitForSeconds(0.75f);
        animator.SetBool("hurt", false);
    }

    IEnumerator Idle()
    {
        yield return new WaitForSeconds(.3f);
        previousFinished = true;
        idled = true;
    }

    IEnumerator Wall(Vector2 moveLocation)
    {
        //hitWall = true;
        //animator.SetBool("wall", true);
        //this.rigidbody2D.velocity = Vector2.zero;
        //this.rigidbody2D.MovePosition(moveLocation);


        yield return new WaitForSeconds(.2f);
        animator.SetBool("wall", false);
        animator.SetBool("charge", false);
        hitWall = false;
    }

    void OnDeath()
    {
        this.audioSource.PlayOneShot(deadSound);
        Instantiate(glob, this.transform.position, new Quaternion(0, 0, 0, 0)).GetComponent<GlobBehavior>().degree = 0;
        Instantiate(glob, this.transform.position, new Quaternion(0, 0, 0, 0)).GetComponent<GlobBehavior>().degree = Mathf.PI / 4;
        Instantiate(glob, this.transform.position, new Quaternion(0, 0, 0, 0)).GetComponent<GlobBehavior>().degree = -Mathf.PI / 4;
        Instantiate(glob, this.transform.position, new Quaternion(0, 0, 0, 0)).GetComponent<GlobBehavior>().degree = Mathf.PI / 2;
        Instantiate(glob, this.transform.position, new Quaternion(0, 0, 0, 0)).GetComponent<GlobBehavior>().degree = -Mathf.PI / 2;
        Instantiate(glob, this.transform.position, new Quaternion(0, 0, 0, 0)).GetComponent<GlobBehavior>().degree = 3 * Mathf.PI / 4;
        Instantiate(glob, this.transform.position, new Quaternion(0, 0, 0, 0)).GetComponent<GlobBehavior>().degree = -3 * Mathf.PI / 4;
        StartCoroutine(FinishLevel());
    }

    IEnumerator FinishLevel()
    {
        dead = true;
        yield return new WaitForSeconds(.5f);
        this.GetComponent<SpriteRenderer>().color = Color.clear;
        yield return new WaitForSeconds(1.5f);

        player.GetComponent<PlayableCharacter>().FinishLevel();
        if (!player.GetComponent<PlayableCharacter>().dead)
        {
            this.audioSource.PlayOneShot(victorySound);
            yield return new WaitForSeconds(4.75f);
            Application.LoadLevel(player.GetComponent<PlayableCharacter>().nextLevel);
        }
    }
}
