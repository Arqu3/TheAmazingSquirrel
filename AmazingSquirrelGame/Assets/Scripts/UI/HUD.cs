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

    Player player;

    string timeFormat;
    string foodFormat;

    private void Awake()
    {
        Current = this;

        player = FindObjectOfType<Player>();

        timeFormat = timeText.text;
        foodFormat = foodText.text;

        UpdateFood(0);
    }

    public void UpdateFood(int amount)
    {
        foodText.text = string.Format(foodFormat, amount, player.foodRequried);

        if (amount >= player.foodRequried) onGameFinished.Invoke();
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
}
