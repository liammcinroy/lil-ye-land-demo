using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using LilYeHelpers;

public class DemoYe : PlayableCharacter
{

    public AudioClip torchOnSound;
    public AudioClip torchEmptySound;
    public AudioClip torchCollectedSound;
    public AudioClip batteryCollectedSound;
    public AudioClip fullBatterySound;

    [HideInInspector]
    public bool torch_on = false;
    public bool has_torch = false;

    public int torch_battery = 3;
    private float turn_on_torch_time = 0;

    public AnimatorOverrideController torch_controller;
    public GameObject torch;

    public Image torch_ui;
    public Sprite[] torch_sprites;

    private Image Fire1;

    private Color torch_color;

    // checkpoint stuff

    // Use this for initialization
    new public void Start()
    {
        base.Start();
        torch_color = Color.white;
        torch_color.a = .5f;
        Fire1 = GameObject.Find("Fire1").GetComponent<Image>();
    }

    // Update is called once per frame
    new public void Update()
    {
        base.Update();
        if (has_torch)
        {
            this.animator.runtimeAnimatorController = torch_controller;

            if (torch_on)
                torch.GetComponent<SpriteRenderer>().color = torch_color;

            else if (((Input.GetButtonDown("Fire1") && this.buttons["Fire1"] == null) || this.buttons["Fire1"].downFrame) && !torch_on)
            {
                if (torch_battery > 0)
                {
                    torch_on = true;
                    can_take_damage = false;
                    turn_on_torch_time = Time.time;
                    this.audioSource.PlayOneShot(torchOnSound);
                }

                else
                    this.audioSource.PlayOneShot(torchEmptySound);
            }

            else
                torch.GetComponent<SpriteRenderer>().color = Color.clear;

            if (Time.time - turn_on_torch_time > 1 && torch_on)
            {
                torch_on = false;
                can_take_damage = true;
                torch_battery--;
            }
        }

        if (Fire1 != null)
        {
            if (has_torch)
                Fire1.color = Color.white;
            else
                Fire1.color = Color.clear;
        }

        foreach (Transform battery in (from obj in Resources.FindObjectsOfTypeAll<Transform>() where obj.tag == "TorchBattery" select obj))
        {
            battery.GetComponent<SpriteRenderer>().color = (torch_battery < 4) ? Color.white : Color.clear;
            battery.GetComponent<PolygonCollider2D>().enabled = torch_battery < 4;
        }
    }

    new public void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if (collision.gameObject.tag == "Torch")
        {
            this.audioSource.PlayOneShot(torchCollectedSound);
            collision.gameObject.GetComponent<Animator>().SetBool("collected", true);
            StartCoroutine(Helpers.Deactivate(collision.gameObject, .33f));
            has_torch = true;
        }
        if (collision.gameObject.tag == "TorchBattery" && torch_battery < 4)
        {
            this.audioSource.PlayOneShot(batteryCollectedSound);
            collision.gameObject.GetComponent<Animator>().SetBool("collected", true);
            StartCoroutine(Helpers.Deactivate(collision.gameObject, .33f));
            torch_battery += 1;
        }
        else if (torch_battery == 4)
            this.audioSource.PlayOneShot(fullBatterySound);
    }

    new public void OnGUI()
    {
        base.OnGUI();
        if (torch_ui != null)
            torch_ui.sprite = torch_sprites[(int)torch_battery];

        if (Fire1 != null)
            Fire1.gameObject.SetActive(has_torch);
    }
}
