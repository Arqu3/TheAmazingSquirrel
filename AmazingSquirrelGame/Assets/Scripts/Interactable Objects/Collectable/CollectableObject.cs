using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (Collider))]
public abstract class CollectableObject : MonoBehaviour, ICollectable
{
    protected virtual void Awake()
    {
        GetComponent<Collider> ().isTrigger = true;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    protected virtual void OnTriggerEnter(Collider col)
    {
        if (!gameObject.activeInHierarchy) return;

        var collector = col.GetComponent<ICollector> ();
        if (collector != null)
        {
            collector.Add (this);
            gameObject.SetActive (false);
        }
    }
}
