using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartScreen : MonoBehaviour
{
    void Start()
    {
        Time.timeScale = 0f;

        StartCoroutine(_InputSequence());
    }

    IEnumerator _InputSequence()
    {
        yield return new WaitUntil(() => Input.anyKeyDown);

        Time.timeScale = 1f;

        Destroy(gameObject);
    }
}
