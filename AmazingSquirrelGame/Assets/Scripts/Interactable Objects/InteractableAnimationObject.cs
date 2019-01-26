using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class InteractableAnimationObject : InteractableObject
{
    #region Public variables

    public enum VariableType
    {
        Int = 0,
        Float = 1,
        Bool = 2,
        Trigger = 3
    }

    [Header("Variable type")]
    [SerializeField]
    VariableType variableType = VariableType.Int;

    [Header("Int (TYPE = INT)")]
    [SerializeField]
    int intValue = 0;

    [Header("Float (TYPE = FLOAT)")]
    [SerializeField]
    float floatValue = 0f;

    [Header("Bool (TYPE = BOOL)")]
    [SerializeField]
    bool boolValue = false;

    #endregion

    #region Components

    Animator animator;

    #endregion

    void Awake()
    {
        animator = GetComponent<Animator>();
    }
}
