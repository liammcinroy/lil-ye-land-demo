using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndScreen : MonoBehaviour {

    public Image[] stars_ui;
    public Sprite empty_star_sprite;
    public Sprite full_star_sprite;

    public Checkpoint checkpoint;
    public string nextLevel;

    private int stars = 0;

    // Use this for initialization
    void Awake()
    {
        GameObject.Find("restart").GetComponent<Button>().onClick.AddListener(delegate {
            Time.timeScale = 1.0f;
            if (checkpoint == null || !checkpoint.hit) Application.LoadLevel(Application.loadedLevel);
            else checkpoint.CauseRespawn();
            Destroy(this.gameObject); });
        stars = GameObject.Find("lil-ye").GetComponent<PlayableCharacter>().stars;
        int score = GameObject.Find("lil-ye").GetComponent<PlayableCharacter>().score;
        if (stars > 0)
        {
            GameObject.Find("continue").GetComponent<Button>().onClick.AddListener(delegate
            {
                Time.timeScale = 1.0f;                
                SceneManager.LoadScene(nextLevel);
            });
            GameObject.Find("bad-title").SetActive(false);
        }

        else
        {
            GameObject.Find("continue").SetActive(false);
            GameObject.Find("title").SetActive(false);
        }

#if UNITY_WEBGL
        Destroy(GameObject.Find("main-menu"));
#else
        GameObject.Find("main-menu").GetComponent<Button>().onClick.AddListener(delegate 
        {
            Time.timeScale = 1.0f;
            Application.LoadLevel("main-menu");
        });
#endif
    }

    private void OnGUI()
    {
        for (int i = 0; i < stars; ++i)
            stars_ui[i].sprite = full_star_sprite;
        for (int i = stars; i < 3; ++i)
            stars_ui[i].sprite = empty_star_sprite;
    }
}
