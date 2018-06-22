using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class ComponentVariableCheckpoint : MonoBehaviour {

    public string componentName;
    public string[] fields;

    public Checkpoint checkpoint;

    private Dictionary<string, object> values;
    private object component;

    void Start() {
        if (checkpoint != null)
        {
            checkpoint.OnCheckpoint += OnCheckpoint;
            checkpoint.OnRespawn += OnRespawn;
        }

        component = this.gameObject.GetComponent(componentName);
        values = new Dictionary<string, object>();
    }

    public void OnCheckpoint()
    {
        foreach (string field in fields)
            values[field] = this.gameObject.GetComponent(componentName).GetType().GetField(field).GetValue(component);
    }

    public void OnRespawn()
    {
        foreach (string field in fields)
            this.gameObject.GetComponent(componentName).GetType().GetField(field).SetValue(component, values[field]);
    }
}
