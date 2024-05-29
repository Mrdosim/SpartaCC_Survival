using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerUI : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    private float remainingTime;
    private bool isTimerActive;

    void Start()
    {
        timerText.text = "";
        isTimerActive = false;
    }

    void Update()
    {
        if (isTimerActive)
        {
            remainingTime -= Time.deltaTime;
            if (remainingTime > 0)
            {
                timerText.text = $"남은 시간: {remainingTime:F1}s";
            }
            else
            {
                timerText.text = "";
                isTimerActive = false;
            }
        }
    }

    public void StartTimer(float duration)
    {
        remainingTime = duration;
        isTimerActive = true;
    }
}
