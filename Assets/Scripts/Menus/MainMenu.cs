using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using LilYeHelpers;

public class MainMenu : MonoBehaviour {

    public Text score;

    private Button play;
    private Button store;
    private GameObject coin;
    private Image gameTitle;

    private GameObject levelSelectCanvas;

    private Vector3 playOrigin;
    private Vector3 restartOrigin;
    private Vector3 storeOrigin;
    private Vector3 coinOrigin;

    private float gameTime;
    private bool clear = true;

	// Use this for initialization
	void Awake () {
        play = GameObject.Find("play").GetComponent<Button>();
        store = GameObject.Find("store").GetComponent<Button>();
        coin = GameObject.Find("coin");
        gameTitle = GameObject.Find("kids-see-ghosts").GetComponent<Image>();
        levelSelectCanvas = GameObject.Find("level-select");
        levelSelectCanvas.SetActive(false);

        playOrigin = play.transform.position;
        storeOrigin = store.transform.position;
        coinOrigin = coin.transform.position;

        play.onClick.AddListener(delegate { GameObject.Find("logo").SetActive(false);
                                            levelSelectCanvas.SetActive(true);
                                            this.gameObject.SetActive(false); });

        store.onClick.AddListener(delegate {
            Application.OpenURL("https://www.kickstarter.com/projects/1538450318/1618577795");
        });

        score.text = CustomPlayerPrefs.GetInt("Score", 0).ToString();

        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }

    // Update is called once per frame
    void Update () {
        play.transform.position = Vector3.Lerp(playOrigin - 30 * (Vector3.right + Vector3.up), playOrigin, Time.timeSinceLevelLoad / 1f);
        store.transform.position = Vector3.Lerp(storeOrigin - 45 * Vector3.up, storeOrigin, Time.timeSinceLevelLoad / 1.5f);
        coin.transform.position = Vector3.Lerp(coinOrigin - 120 * Vector3.right, coinOrigin, Time.timeSinceLevelLoad / 2);

        Color col = Color.white;
        col.a = Mathf.Lerp(0, 1.0f, (Time.timeSinceLevelLoad - gameTime) / 2);
        if (!clear)
            col.a = 1 - col.a;
        if (Time.timeSinceLevelLoad - gameTime > 2)
        {
            clear = !clear;
            gameTime = Time.timeSinceLevelLoad;
        }
        gameTitle.color = col;
    }
}
