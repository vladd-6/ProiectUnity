using UnityEngine;
using TMPro;

public class LevelTimer : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI bestTimeText;

    private float currentTime = 0f;
    private bool isRunning = true;
    private string sceneName;

    void Start()
    {
        sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        UpdateBestTimeDisplay();
    }

    void Update()
    {
        if (isRunning)
        {
            currentTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            timerText.text = "Time: " + FormatTime(currentTime);
        }
    }

    private void UpdateBestTimeDisplay()
    {
        if (bestTimeText != null)
        {
            float bestTime = GetBestTime();
            if (bestTime > 0)
            {
                bestTimeText.text = "Best: " + FormatTime(bestTime);
            }
            else
            {
                bestTimeText.text = "Best: N/A";
            }
        }
    }

    private string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int milliseconds = Mathf.FloorToInt((time * 100f) % 100f);
        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public float GetCurrentTime()
    {
        return currentTime;
    }

    public float GetBestTime()
    {
        string key = "BestTime_" + sceneName;
        return PlayerPrefs.GetFloat(key, -1f);
    }

    public bool UpdateBestTime()
    {
        float bestTime = GetBestTime();
        if (bestTime < 0 || currentTime < bestTime)
        {
            string key = "BestTime_" + sceneName;
            PlayerPrefs.SetFloat(key, currentTime);
            PlayerPrefs.Save();
            UpdateBestTimeDisplay();
            return true; // New record
        }
        return false; // Not a new record
    }

    public void ResetTimer()
    {
        currentTime = 0f;
        isRunning = true;
        UpdateTimerDisplay();
    }
}
