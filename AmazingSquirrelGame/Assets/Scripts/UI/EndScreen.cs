using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndScreen : MonoBehaviour
{
    [SerializeField]
    Button restartButton;
    [SerializeField]
    TMP_Text gatheredText;
    [SerializeField]
    TMP_Text timeRemainingText;

    string gatheredFormat;
    string timeRemainingFormat;

    private void Awake()
    {
        gatheredFormat = gatheredText.text;
        timeRemainingFormat = timeRemainingText.text;

        restartButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            Debug.Log("Restart");
        });
    }

    private void Start()
    {
        HUD.Current.onGameFinished.AddListener(Activate);

        gameObject.SetActive(false);
    }

    public void Activate()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        gameObject.SetActive(true);
        Time.timeScale = 0f;
        gatheredText.text = string.Format(gatheredFormat, FindObjectOfType<Player>().GetFoodCount(), FindObjectOfType<Player>().foodRequried);
        timeRemainingText.text = string.Format(timeRemainingFormat, HUD.FormatAsTime(TimeManager.Instance.GetRemainingTime()));
    }
}
