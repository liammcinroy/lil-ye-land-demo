using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using LilYeHelpers;

public class PauseScreen : MonoBehaviour {

    public delegate void ChangePref();
    public event ChangePref PrefsChanged;

    public Sprite muteSfxActiveSprite;
    public Sprite muteMusicActiveSprite;
    public Sprite muteSfxInactiveSprite;
    public Sprite muteMusicInactiveSprite;

    private Image muteSfxImg;
    private Image muteMusicImg;

    private bool muteSfx;
    private bool muteMusic;

    void Awake()
    {
        muteSfx = CustomPlayerPrefs.GetInt("MuteSfx", 0) == 1;
        muteMusic = CustomPlayerPrefs.GetInt("MuteMusic", 0) == 1;

        muteSfxImg = GameObject.Find("mute-sfx").GetComponent<Image>();
        muteMusicImg = GameObject.Find("mute-music").GetComponent<Image>();


        GameObject.Find("mute-sfx").GetComponent<Button>().onClick.AddListener(delegate { muteSfx = !muteSfx;
                                                                                          CustomPlayerPrefs.SetInt("MuteSfx", muteSfx ? 1 : 0);
                                                                                          GameObject.FindGameObjectWithTag("Player").GetComponent<PlayableCharacter>().muteSfx = muteSfx;
                                                                                          PrefsChanged.Invoke();
                                                                                          CustomPlayerPrefs.Save(); });
        GameObject.Find("mute-music").GetComponent<Button>().onClick.AddListener(delegate { muteMusic = !muteMusic;
                                                                                            CustomPlayerPrefs.SetInt("MuteMusic", muteMusic ? 1 : 0);
                                                                                            GameObject.FindGameObjectWithTag("Player").GetComponent<PlayableCharacter>().muteMusic = muteMusic;
                                                                                            PrefsChanged.Invoke();
                                                                                            CustomPlayerPrefs.Save(); });
        GameObject.Find("resume").GetComponent<Button>().onClick.AddListener(delegate { Time.timeScale = 1.0f; Destroy(this.gameObject); });
        GameObject.Find("restart").GetComponent<Button>().onClick.AddListener(delegate { Time.timeScale = 1.0f; Application.LoadLevel(Application.loadedLevel); });

#if UNITY_WEBGL
        Destroy(GameObject.Find("main-menu"));
#else
        GameObject.Find("main-menu").GetComponent<Button>().onClick.AddListener(delegate { Time.timeScale = 1.0f; Application.LoadLevel("main-menu"); });
#endif
    }

    private void OnGUI()
    {
        muteSfxImg.sprite = muteSfx ? muteSfxActiveSprite : muteSfxInactiveSprite;
        muteMusicImg.sprite = muteMusic ? muteMusicActiveSprite : muteMusicInactiveSprite;
    }
}
