using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using LilYeHelpers;

public class PlayableCharacter : Character {

    private Camera followCamera;
    public float camera_max_velocity;

    public AudioClip coinCollectedSound;
    public AudioClip hitSound;
    public AudioClip deadSound;
    public AudioClip victorySound;

    public int max_health;

    public Text score_text;

    public Image[] health_hearts_ui;
    public Sprite empty_heart_sprite;
    public Sprite full_heart_sprite;

    public Image[] stars_ui;
    public Sprite empty_star_sprite;
    public Sprite full_star_sprite;

    public Button pauseButton;
    public GameObject pauseScreen;
    public GameObject endScreen;

    public Checkpoint checkpoint;
    public string nextLevel;
    public string saveLevel;

    [HideInInspector]
    public int health = 0;
    [HideInInspector]
    public int score = 0;
    [HideInInspector]
    public int stars = 0;
    [HideInInspector]
    public bool dead = false;

    [HideInInspector]
    public int oldStars = 0;

    [HideInInspector]
    public bool muteSfx;
    [HideInInspector]
    public bool muteMusic;

    private AudioSource music;

    private bool camera_moving = false;
    private Vector3 camera_destination = Vector3.zero;

    private float last_ghost_hit_time = 0;
    private int last_ghost_hit_id = 0;

    private float last_move = 0;

    private int num_coins_start;

    protected Dictionary<string, MobileButton> buttons = new Dictionary<string, MobileButton>();

    protected AudioSource audioSource;

    protected bool can_take_damage = true;

    new public void Start()
    {
        base.Start();
        muteSfx = CustomPlayerPrefs.GetInt("MuteSfx", 0) > 0;
        muteMusic = CustomPlayerPrefs.GetInt("MuteMusic", 0) > 0;

        music = GameObject.FindGameObjectWithTag("Music").GetComponent<AudioSource>();
        audioSource = this.GetComponent<AudioSource>();
        followCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

        UpdateAudio();

#if UNITY_EDITOR
        max_health = 500;
#endif

        health = max_health;
        num_coins_start = GameObject.FindGameObjectsWithTag("Coin").Length;

        if (GameObject.Find("boss") != null)
            oldStars = CustomPlayerPrefs.GetInt("LastStars", 0);

#if UNITY_WEBGL || UNITY_EDITOR
        foreach (GameObject mobile_ui_elem in GameObject.FindGameObjectsWithTag("Mobile UI"))
            Destroy(mobile_ui_elem);
#else
                Screen.orientation = ScreenOrientation.LandscapeLeft;
#endif

        foreach (GameObject mobile_ui_elem in GameObject.FindGameObjectsWithTag("Mobile UI"))
            buttons.Add(mobile_ui_elem.name, mobile_ui_elem.GetComponent<MobileButton>());

        pauseButton.onClick.AddListener(delegate { if (GameObject.FindGameObjectWithTag("PauseScreen") == null) {
                                                       Time.timeScale = 0;
                                                       Instantiate(pauseScreen, GameObject.Find("UI").transform).GetComponent<PauseScreen>().PrefsChanged += UpdateAudio; }
                                                   else {
                                                       Time.timeScale = 1;
                                                       Destroy(GameObject.FindGameObjectWithTag("PauseScreen")); } });

#if UNITY_WEBGL
        nextLevel = "webgl-ad";
#endif

        if (checkpoint != null)
            checkpoint.OnRespawn += delegate { GameObject.Find("music").GetComponent<AudioSource>().mute = muteMusic; };
    }

    public override void get_animator_params()
    {
        moving = Mathf.Abs(Input.GetAxis("Horizontal")) > .001f || buttons["Right"].pressed || buttons["Left"].pressed;
        if (moving)
            facing_left = base.rigidbody2D.velocity.x < -.001f; // if 0, then glitches
    }

    public override void get_motion_params()
    {
        jump = Input.GetButtonDown("Jump") || buttons["Jump"].downFrame;
        if (Mathf.Abs(Input.GetAxis("Horizontal")) > .001f)
            velocity_direction = Mathf.Sign(Input.GetAxis("Horizontal"));
        else if (buttons["Right"].pressed)
            velocity_direction = 1;
        else if (buttons["Left"].pressed)
            velocity_direction = -1;
        else 
            velocity_direction = 0.0f;
    }

    new public void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if (collision.gameObject.tag == "Enemy")
        {
            if ((collision.gameObject.GetInstanceID() != last_ghost_hit_id || Time.time - last_ghost_hit_time > 1)
                && this.can_take_damage)
            {
                audioSource.PlayOneShot(hitSound);
                base.animator.SetBool("hurt", true);
                health--;
                last_ghost_hit_time = Time.time;
                last_ghost_hit_id = collision.gameObject.GetInstanceID();
            }
        }
        else if (collision.gameObject.tag == "Coin")
        {
            audioSource.PlayOneShot(coinCollectedSound);
            collision.gameObject.GetComponent<Animator>().SetBool("collected", true);
            StartCoroutine(Helpers.Deactivate(collision.gameObject, .33f));
            score++;
        }

        else if (collision.gameObject.tag == "Heart" && health < max_health)
        {
            audioSource.PlayOneShot(coinCollectedSound);
            collision.gameObject.GetComponent<Animator>().SetBool("collected", true);
            StartCoroutine(Helpers.Deactivate(collision.gameObject, .33f));
            health++;
        }

        else if (collision.gameObject.tag == "Exit")
        {
            FinishLevel();
        }
    }

    new public void Update()
    {
        base.Update();

        // camera

        Vector3 screen_point = followCamera.WorldToScreenPoint(this.transform.position);

        if (!camera_moving && (screen_point.x >= followCamera.pixelWidth / 3.0f * 2 || screen_point.x <= followCamera.pixelWidth / 3.0f
            || screen_point.y >= followCamera.pixelHeight / 3.0f * 2 || screen_point.y <= followCamera.pixelHeight / 3.0f))
        {
            camera_moving = true;
        }

        else if (camera_moving)
        {
            camera_destination = this.transform.position;
            camera_destination.z = followCamera.transform.position.z;
            if (Vector3.Distance(camera_destination, followCamera.transform.position) < .001f)
                camera_moving = false;
            else
                followCamera.transform.position = Vector3.MoveTowards(followCamera.transform.position, camera_destination, Time.deltaTime * (camera_max_velocity * .1f * this.rigidbody2D.velocity.magnitude));
        }

        // special idle
        if (moving || jump)
            last_move = Time.time;

        this.animator.SetBool("idle-special", Time.time - last_move > 3);
        if (Time.time - last_move > .4f)
            last_move = Time.time;

        // hurt
        if (Time.time - last_ghost_hit_time > .4f)
            base.animator.SetBool("hurt", false);

        // if it fell off you lose
        if (!dead && (this.transform.position.y < 0 || health <= 0))
        {
            dead = true;
            base.animator.SetBool("hurt", true);
            audioSource.PlayOneShot(deadSound);
            Invoke("FinishLevel", .4f);
        }

        foreach (GameObject heart in GameObject.FindGameObjectsWithTag("Heart"))
        {
            heart.GetComponent<SpriteRenderer>().color = (health < max_health) ? Color.white : Color.clear;
            heart.GetComponent<PolygonCollider2D>().enabled = health < max_health;
        }
    }

    public void OnGUI()
    {
        score_text.text = score.ToString();
        for (int i = 0; i < health; ++i)
            health_hearts_ui[i].sprite = full_heart_sprite;
        for (int i = health; i < max_health && i > -1; ++i)
            health_hearts_ui[i].sprite = empty_heart_sprite;

        if (oldStars == 0)
        {
            float prop_coin = score / (float)num_coins_start;
            stars = (prop_coin > .5f && health > 0) ? (prop_coin > .8f ? 2 : 1) : 0;
        }
        else
            stars = oldStars;
        for (int i = 0; i < stars; ++i)
            stars_ui[i].sprite = full_star_sprite;
        for (int i = stars; i < 3; ++i)
            stars_ui[i].sprite = empty_star_sprite;
    }

    public void FinishLevel()
    {
        if (oldStars == 0)
        {
            float prop_coin = score / (float)num_coins_start;
            stars = (prop_coin > .5f) ? (prop_coin > .8f ? 2 : 1) : 0;
            if (health > 0)
                stars++;
        }
        else
            stars = oldStars;
        if (this.transform.position.y < 0 || health <= 0)
            stars = 0;
        else
        {
            this.audioSource.PlayOneShot(victorySound);
            CustomPlayerPrefs.SetString("Level", nextLevel);
            CustomPlayerPrefs.SetInt("Score", score + CustomPlayerPrefs.GetInt("Score", 0));
            CustomPlayerPrefs.SetInt("LastStars", stars);
            Helpers.ChangeUserProgress(saveLevel == null ? Application.loadedLevelName : saveLevel, stars);
        }

        if (nextLevel.Contains("boss") && stars > 0 && !Application.loadedLevelName.Contains("boss"))
            Application.LoadLevel(nextLevel);

        else if (!Application.loadedLevelName.Contains("boss") || stars == 0)
        {
            EndScreen end = Instantiate(endScreen, GameObject.Find("UI").transform).GetComponent<EndScreen>();
            end.checkpoint = checkpoint;
            end.nextLevel = nextLevel;
            Time.timeScale = 0;
            GameObject.Find("music").GetComponent<AudioSource>().mute = true;
        }
    }

    private void UpdateAudio()
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Music"))
            Debug.Log(obj.transform.parent.name);
        music.mute = muteMusic;
        foreach (AudioSource src in GameObject.FindObjectsOfType<AudioSource>())
            if (src.GetInstanceID() != music.GetInstanceID())
                src.mute = muteSfx;
    }
}
