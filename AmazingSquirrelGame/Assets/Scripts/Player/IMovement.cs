﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMovement
{
    void Lock(Vector3 forward);
    void Unlock();

    Coroutine AddForceDirectional(float strength, float duration, Vector3 direction);

    void StopDirectional();

    Vector3 InputVector
    {
        get;
    }

    bool Enabled
    {
        get;
        set;
    }
}
