using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionCheckpoint : MonoBehaviour {

    public Checkpoint checkpoint;

    private bool saveActive;
    private Vector3 savePosition;

	// Use this for initialization
	void Start () {
        if (checkpoint != null)
        {
            checkpoint.OnCheckpoint += OnCheckpoint;
            checkpoint.OnRespawn += Respawn;
        }
    }
	
    private void OnCheckpoint()
    {
        saveActive = this.gameObject.activeSelf;
        savePosition = this.gameObject.transform.position;
    }
	
    private void Respawn()
    {
        this.gameObject.SetActive(saveActive);
        this.gameObject.transform.position = savePosition;
    }

}
