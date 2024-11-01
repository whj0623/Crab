using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public Timer timer;
    public Text timerText;
    public float timeRemaining; 
    public bool timerIsRunning = false;
    public MiniGameManager minigameManager;
    private void Update()
    {
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                UpdateTimerDisplay();
                timeRemaining -= Time.deltaTime;
            }
            else
            {
                timeRemaining = 0;
                timerIsRunning = false;
                if (timer != null)// freezetimer의 경우
                {
                    if (minigameManager != null)
                        minigameManager.GameStart();
                    timer.timerIsRunning = true;
                    this.enabled = false;
                }
                else 
                {
                    if (minigameManager != null)
                        minigameManager.GameOver();
                }
            }
        }
    }

    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);
        timerText.text = $"{minutes}:{seconds:D2}";
    }
}
