using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerDeathHandler : MonoBehaviour
{
    [Header("Fall Detection")]
    [SerializeField] private float deathY = -100f;

    private HealthController healthController;
    private bool isDead = false;

    void Start()
    {
        healthController = GetComponent<HealthController>();
    }

    void Update()
    {
        // Check if player fell off the map
        if (!isDead && transform.position.y < deathY)
        {
            TriggerDeath();
        }

        // Check if player died from damage
        if (!isDead && healthController != null && healthController.parameters.toughness <= 0)
        {
            TriggerDeath();
        }
    }

    private void TriggerDeath()
    {
        if (isDead) return;
        isDead = true;
        StartCoroutine(PlayerDeathSequence());
    }

    private IEnumerator PlayerDeathSequence()
    {
        // Show cursor so player can click respawn button
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Find and freeze camera position before disabling scripts
        Camera playerCamera = GetComponentInChildren<Camera>();
        Vector3 frozenCameraPosition = Vector3.zero;
        Quaternion frozenCameraRotation = Quaternion.identity;
        if (playerCamera != null)
        {
            frozenCameraPosition = playerCamera.transform.position;
            frozenCameraRotation = playerCamera.transform.rotation;
        }

        // Disable physics
        if (TryGetComponent<Collider>(out var collider))
        {
            collider.enabled = false;
        }
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Disable all player control scripts
        MonoBehaviour[] playerScripts = GetComponentsInChildren<MonoBehaviour>();
        foreach (MonoBehaviour script in playerScripts)
        {
            if (script == this) continue; // Don't disable this script
            
            if (script.GetType().Name.Contains("Player") ||
                script.GetType().Name.Contains("Controller") ||
                script.GetType().Name.Contains("Gun") ||
                script.GetType().Name.Contains("Camera"))
            {
                script.enabled = false;
            }
        }

        // Restore camera position after disabling scripts
        if (playerCamera != null)
        {
            playerCamera.transform.position = frozenCameraPosition;
            playerCamera.transform.rotation = frozenCameraRotation;
        }

        GameObject deathScreen = CreateDeathScreen();

        Image overlayImage = deathScreen.GetComponentInChildren<Image>();
        TextMeshProUGUI deathText = deathScreen.GetComponentInChildren<TextMeshProUGUI>();
        Image buttonImage = deathScreen.transform.Find("RespawnButton")?.GetComponent<Image>();
        TextMeshProUGUI buttonText = deathScreen.transform.Find("RespawnButton/Text")?.GetComponent<TextMeshProUGUI>();

        if (deathText != null)
        {
            deathText.alpha = 0;
        }

        if (buttonImage != null)
        {
            Color buttonColor = buttonImage.color;
            buttonColor.a = 0;
            buttonImage.color = buttonColor;
        }

        if (buttonText != null)
        {
            buttonText.alpha = 0;
        }

        float fadeDuration = 2f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float fadeProgress = elapsedTime / fadeDuration;

            if (overlayImage != null)
            {
                Color overlayColor = overlayImage.color;
                overlayColor.a = fadeProgress;
                overlayImage.color = overlayColor;
            }

            if (deathText != null)
            {
                if (fadeProgress > 0.5f)
                {
                    deathText.alpha = (fadeProgress - 0.5f) * 2f;
                }
            }

            // Fade in button after text, starting at 70% progress
            if (fadeProgress > 0.7f)
            {
                float buttonFadeProgress = (fadeProgress - 0.7f) / 0.3f;
                if (buttonImage != null)
                {
                    Color buttonColor = buttonImage.color;
                    buttonColor.a = buttonFadeProgress;
                    buttonImage.color = buttonColor;
                }
                if (buttonText != null)
                {
                    buttonText.alpha = buttonFadeProgress;
                }
            }

            yield return null;
        }

        if (overlayImage != null)
        {
            Color finalColor = overlayImage.color;
            finalColor.a = 1f;
            overlayImage.color = finalColor;
        }

        if (deathText != null)
        {
            deathText.alpha = 1f;
        }
    }

    private GameObject CreateDeathScreen()
    {
        Canvas targetCanvas = null;

        // Try to find canvas from health bar
        if (healthController != null && healthController.healthBar != null)
        {
            targetCanvas = healthController.healthBar.GetComponentInParent<Canvas>();
        }

        if (targetCanvas == null)
        {
            Canvas[] allCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            foreach (Canvas c in allCanvases)
            {
                if (c.name.Contains("UI") || c.name.Contains("HUD") || c.name.Contains("Canvas"))
                {
                    targetCanvas = c;
                    break;
                }
            }
        }

        GameObject deathScreenObject = new("DeathScreen");
        deathScreenObject.transform.SetParent(targetCanvas.transform, false);

        Image overlayImage = deathScreenObject.AddComponent<Image>();
        overlayImage.color = new Color(1f, 0f, 0f, 0f);

        RectTransform rectTransform = deathScreenObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        GameObject textObject = new("DeathText");
        textObject.transform.SetParent(deathScreenObject.transform, false);

        TextMeshProUGUI deathText = textObject.AddComponent<TextMeshProUGUI>();
        deathText.text = "You died\nBetter luck next time";
        deathText.fontSize = 60;
        deathText.alignment = TextAlignmentOptions.Center;
        deathText.color = new Color(1f, 1f, 1f, 0f);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.one * 0.5f;
        textRect.anchorMax = Vector2.one * 0.5f;
        textRect.sizeDelta = new Vector2(800, 200);
        textRect.anchoredPosition = Vector2.zero;

        CreateRespawnButton(deathScreenObject);

        return deathScreenObject;
    }

    private void CreateRespawnButton(GameObject deathScreenObject)
    {
        GameObject buttonObject = new("RespawnButton");
        buttonObject.transform.SetParent(deathScreenObject.transform, false);

        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0f);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonImage;

        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        colors.pressedColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        button.colors = colors;

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.3f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.3f);
        buttonRect.sizeDelta = new Vector2(200, 60);
        buttonRect.anchoredPosition = Vector2.zero;

        GameObject buttonTextObject = new("Text");
        buttonTextObject.transform.SetParent(buttonObject.transform, false);

        TextMeshProUGUI buttonText = buttonTextObject.AddComponent<TextMeshProUGUI>();
        buttonText.text = "Respawn";
        buttonText.fontSize = 40;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = new Color(1f, 1f, 1f, 0f);

        RectTransform buttonTextRect = buttonTextObject.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;

        button.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name));
    }
}
