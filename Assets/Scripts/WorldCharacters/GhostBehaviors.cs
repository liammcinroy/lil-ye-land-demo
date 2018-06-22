using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LilYeHelpers;

public class GhostBehaviors : Character
{

    private enum PatrolBehaviors
    {
        Idle,
        ChooseRandom,
        PursueCharacter
    }

    public bool patrol_x = true;
    public float patrol_radius;
    public float pursue_radius;

    public bool die_on_touch;

    public GameObject player;

    [HideInInspector]
    public bool can_damage = true;
    private float light_intensity;

    private PatrolBehaviors current_behavior = PatrolBehaviors.Idle;
    private Vector3 root_position = Vector3.zero;
    private Vector3 start_position = Vector3.zero;
    private Vector3 target_position = Vector3.zero;
    private float behavior_duration = 0;
    private float start_behavior_time = 0;

    new public void Start()
    {
        base.Start();
        root_position = this.transform.position;
    }

    public override void get_animator_params()
    {
        moving = (current_behavior != PatrolBehaviors.Idle);
        facing_left = (target_position.x < start_position.x);
    }

    public override void get_motion_params()
    {
        if (Time.time - start_behavior_time > behavior_duration
            || (Vector3.Distance(this.transform.position, target_position) < .01f && current_behavior != PatrolBehaviors.Idle))
        {
            start_position = this.transform.position;
            start_behavior_time = Time.time;

            if (Random.Range(0, 10) < 7)
            {
                current_behavior = PatrolBehaviors.ChooseRandom;
                if (patrol_x)
                    target_position = root_position + new Vector3(patrol_radius * Mathf.Sign(root_position.x - this.transform.position.x), 0, 0);
                else
                    target_position = root_position + new Vector3(0, patrol_radius * Mathf.Sign(root_position.y - this.transform.position.y), 0);
                behavior_duration = Vector3.Distance(target_position, this.transform.position) / base.max_velocity;
            }

            else
            {
                current_behavior = PatrolBehaviors.Idle;
                target_position = start_position = transform.position;
                behavior_duration = Random.Range(.5f, 2.5f);
            }
        }

        // If they just came into vision
        if (Vector3.Distance(player.transform.position, this.transform.position) < pursue_radius)
        {
            base.animator.SetBool("spotted", current_behavior != PatrolBehaviors.PursueCharacter);
            current_behavior = PatrolBehaviors.PursueCharacter;
            target_position = player.transform.position;
            behavior_duration = Vector3.Distance(player.transform.position, this.transform.position) / base.max_velocity;
            start_position = this.transform.position;
            start_behavior_time = Time.time;
        }

        if (light_intensity > 5)
        {
            can_damage = false;
            base.animator.SetBool("dead", true);
            player.GetComponent<AudioSource>().Play();
            StartCoroutine(Helpers.Deactivate(this.gameObject, .5f));
        }

        else if (light_intensity > .15f)
            start_behavior_time += Time.deltaTime; // adjust slerp since we're idling waiting, keep same animation but we're not moving
        else if (current_behavior == PatrolBehaviors.PursueCharacter)
            transform.position = Vector3.MoveTowards(this.transform.position, player.transform.position, Time.deltaTime * base.max_velocity);
        else if (behavior_duration > 0 && current_behavior != PatrolBehaviors.Idle)
            transform.position = Vector3.Slerp(start_position, target_position, (Time.time - start_behavior_time) / behavior_duration);
    }

    new public void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if (collision.gameObject.tag == "Player")
        {
            if (die_on_touch)
            {
                base.animator.SetBool("dead", true);
                StartCoroutine(Helpers.Deactivate(this.gameObject, .5f));
            }
            else
            {
                base.animator.SetBool("spotted", true);
            }
        }
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "TorchLight" && player.GetComponent<DemoYe>().torch_on)
            light_intensity = 10;
        else if (collision.tag == "Light" && collision.gameObject.GetComponent<DemoIntermittentLight>().on)
            light_intensity = 1;
    }

    new public void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);
        base.animator.SetBool("spotted", false);
        light_intensity = 0;
    }

    public float LightIntensity()
    {
        Light[] lights = FindObjectsOfType<Light>();

        float max_intensity = 0;
        foreach (Light light in lights)
        {
            if (light.gameObject.name == "follow light")
                continue;

            RaycastHit2D hit = Physics2D.Raycast(getXY(light.transform.position), getXY(this.transform.position - light.transform.position), light.range * (180 - light.spotAngle) / 180.0f);
            if (hit.collider != null)
            {
                if (hit.collider.gameObject == this.gameObject)
                {
                    float intensity = Mathf.Clamp(Mathf.Pow(hit.distance * (180 - light.spotAngle) / 180.0f, -2) * light.intensity, 0, light.intensity);
                    if (intensity > max_intensity)
                        max_intensity = intensity;
                }
            }
        }

        return max_intensity;
    }
}
