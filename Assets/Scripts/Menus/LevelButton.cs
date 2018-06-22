using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using LilYeHelpers;

public class LevelButton : MonoBehaviour {

    public string prevName;
    public string levelName;

    public GameObject selectedMask;
    public GameObject lockedMask;

    public GameObject[] stars;

    public static string selected = null;
    public static DictionaryOfStringAndInt progress = null;


    // Use this for initialization
    void Awake () {
        progress = Helpers.GetUserProgess();
        selected = null;
        this.GetComponent<Button>().onClick.AddListener(processClick);
        selectedMask.GetComponent<Button>().onClick.AddListener(delegate { Application.LoadLevel(levelName); });

        for (int i = progress[levelName]; i < 3; ++i)
            stars[i].SetActive(false);

        if (progress[prevName] > 0)
            lockedMask.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
        selectedMask.SetActive(selected == levelName);
	}

    void processClick()
    {
        if (!lockedMask.activeInHierarchy)
            selected = levelName;
    }
}
