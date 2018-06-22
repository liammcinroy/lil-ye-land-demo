using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour {

    [HideInInspector]
    public bool hit = false;

    public delegate void Respawn();
    public event Respawn OnRespawn;
    public event Respawn OnCheckpoint;

    private int numResets = 0;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && !hit)
        {
            hit = true;
            this.GetComponent<AudioSource>().Play();
            this.GetComponent<Animator>().SetBool("hit", true);
            OnCheckpoint.Invoke();
        }
    }

    public void CauseRespawn()
    {
        numResets++;
        if (numResets <= 3)
            OnRespawn.Invoke();
        else
            Application.LoadLevel(Application.loadedLevel);
    }
}
