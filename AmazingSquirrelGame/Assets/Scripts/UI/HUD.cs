using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class HUD : MonoBehaviour
{
    [SerializeField]
    TMP_Text timeText;
    [SerializeField]
    TMP_Text foodText;

    public static HUD Current { get; private set; }

    public readonly UnityEvent onGameFinished = new UnityEvent();

    string timeFormat;
    string foodFormat;

    private CollectableFood[] availableFood;

    private void Awake()
    {
        Current = this;

        timeFormat = timeText.text;
        foodFormat = foodText.text;

        availableFood = FindObjectsOfType<CollectableFood>();

        UpdateFood(0);
    }

    public void UpdateFood(int amount)
    {
        foodText.text = string.Format(foodFormat, amount, availableFood.Length);

        if (amount >= availableFood.Length) onGameFinished.Invoke();
    }

    public void UpdateTime(float secondsRemaining)
    {
        timeText.text = FormatAsTime(secondsRemaining);
    }

    public static string FormatAsTime(float secondsRemaining)
    {
        string minutes = ((int)secondsRemaining / 60).ToString();
        string seconds = ((int)secondsRemaining % 60).ToString("00");
        return minutes + ":" + seconds;
    }

    public int GetAvailableFoodCount()
    {
        return availableFood.Length;
    }
}
