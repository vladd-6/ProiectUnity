using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimerUISetup : MonoBehaviour
{
    private LevelTimer levelTimer;

    void Start()
    {
        SetupTimerUI();
    }

    private void SetupTimerUI()
    {
        // Find or create canvas
        Canvas canvas = FindMainCanvas();
        if (canvas == null)
        {
            Debug.LogError("No canvas found for timer UI!");
            return;
        }

        // Create timer container
        GameObject timerContainer = new GameObject("TimerUI");
        timerContainer.transform.SetParent(canvas.transform, false);

        RectTransform containerRect = timerContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0, 1);
        containerRect.anchorMax = new Vector2(0, 1);
        containerRect.pivot = new Vector2(0, 1);
        containerRect.anchoredPosition = new Vector2(20, -20);
        containerRect.sizeDelta = new Vector2(300, 100);
        
        // Make sure timer is always rendered on top
        timerContainer.transform.SetAsLastSibling();

        // Create current time text
        GameObject timerTextObj = new GameObject("CurrentTime");
        timerTextObj.transform.SetParent(timerContainer.transform, false);

        TextMeshProUGUI timerText = timerTextObj.AddComponent<TextMeshProUGUI>();
        timerText.text = "Time: 00:00.00";
        timerText.fontSize = 32;
        timerText.color = Color.white;
        timerText.alignment = TextAlignmentOptions.TopLeft;

        RectTransform timerRect = timerTextObj.GetComponent<RectTransform>();
        timerRect.anchorMin = new Vector2(0, 1);
        timerRect.anchorMax = new Vector2(1, 1);
        timerRect.pivot = new Vector2(0, 1);
        timerRect.anchoredPosition = new Vector2(0, 0);
        timerRect.sizeDelta = new Vector2(0, 40);

        // Create best time text
        GameObject bestTimeTextObj = new GameObject("BestTime");
        bestTimeTextObj.transform.SetParent(timerContainer.transform, false);

        TextMeshProUGUI bestTimeText = bestTimeTextObj.AddComponent<TextMeshProUGUI>();
        bestTimeText.text = "Best: N/A";
        bestTimeText.fontSize = 28;
        bestTimeText.color = new Color(1f, 0.84f, 0f); // Gold color
        bestTimeText.alignment = TextAlignmentOptions.TopLeft;

        RectTransform bestTimeRect = bestTimeTextObj.GetComponent<RectTransform>();
        bestTimeRect.anchorMin = new Vector2(0, 1);
        bestTimeRect.anchorMax = new Vector2(1, 1);
        bestTimeRect.pivot = new Vector2(0, 1);
        bestTimeRect.anchoredPosition = new Vector2(0, -45);
        bestTimeRect.sizeDelta = new Vector2(0, 35);

        // Add outline for better visibility
        Outline timerOutline = timerTextObj.AddComponent<Outline>();
        timerOutline.effectColor = Color.black;
        timerOutline.effectDistance = new Vector2(1, -1);

        Outline bestOutline = bestTimeTextObj.AddComponent<Outline>();
        bestOutline.effectColor = Color.black;
        bestOutline.effectDistance = new Vector2(1, -1);

        // Setup LevelTimer component
        levelTimer = gameObject.AddComponent<LevelTimer>();
        levelTimer.timerText = timerText;
        levelTimer.bestTimeText = bestTimeText;
    }

    private Canvas FindMainCanvas()
    {
        Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        
        // Try to find a main UI canvas (not health bar)
        foreach (Canvas c in allCanvases)
        {
            if (!c.name.Contains("HealthBar") && (c.name.Contains("UI") || c.name.Contains("HUD") || c.name.Contains("Canvas")))
            {
                return c;
            }
        }
        
        // Fallback to any non-HealthBar canvas
        foreach (Canvas c in allCanvases)
        {
            if (!c.name.Contains("HealthBar"))
            {
                return c;
            }
        }
        
        // Last resort: use first canvas
        if (allCanvases.Length > 0)
        {
            return allCanvases[0];
        }

        return null;
    }

    public LevelTimer GetLevelTimer()
    {
        return levelTimer;
    }
}
