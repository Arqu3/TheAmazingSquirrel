using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    void Interact (InteractData data);

    GameObject GetGameObject();
}

public class InteractData
{
    public GameObject source;
}
