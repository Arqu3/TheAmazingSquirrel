using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class InteractableObject : MonoBehaviour, IInteractable
{
    public virtual void Interact(InteractData data)
    {

        Destroy(gameObject);
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
}
