using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerWinHandler : MonoBehaviour
{
    private bool hasWon = false;
    private CharacterController characterController;
    private HealthController healthController;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        healthController = GetComponent<HealthController>();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!hasWon && hit.gameObject.layer == LayerMask.NameToLayer("Finish"))
        {
            Debug.Log("Player won");
            TriggerWin();
        }
    }

    private void TriggerWin()
    {
        if (hasWon) return;
        hasWon = true;
        StartCoroutine(PlayerWinSequence());
    }

    private IEnumerator PlayerWinSequence()
    {
        // Show cursor so player can click play again button
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

        GameObject winScreen = CreateWinScreen();

        Image overlayImage = winScreen.GetComponentInChildren<Image>();
        TextMeshProUGUI winText = winScreen.GetComponentInChildren<TextMeshProUGUI>();
        Image buttonImage = winScreen.transform.Find("PlayAgainButton")?.GetComponent<Image>();
        TextMeshProUGUI buttonText = winScreen.transform.Find("PlayAgainButton/Text")?.GetComponent<TextMeshProUGUI>();

        if (winText != null)
        {
            winText.alpha = 0;
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

            if (winText != null)
            {
                if (fadeProgress > 0.5f)
                {
                    winText.alpha = (fadeProgress - 0.5f) * 2f;
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

        if (winText != null)
        {
            winText.alpha = 1f;
        }
    }

    private GameObject CreateWinScreen()
    {
        Canvas targetCanvas = null;

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

        GameObject winScreenObject = new("WinScreen");
        winScreenObject.transform.SetParent(targetCanvas.transform, false);

        Image overlayImage = winScreenObject.AddComponent<Image>();
        overlayImage.color = new Color(0f, 1f, 0f, 0f); // Green overlay for win

        RectTransform rectTransform = winScreenObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        GameObject textObject = new("WinText");
        textObject.transform.SetParent(winScreenObject.transform, false);

        TextMeshProUGUI winText = textObject.AddComponent<TextMeshProUGUI>();
        winText.text = "You won!";
        winText.fontSize = 60;
        winText.alignment = TextAlignmentOptions.Center;
        winText.color = new Color(1f, 1f, 1f, 0f);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.one * 0.5f;
        textRect.anchorMax = Vector2.one * 0.5f;
        textRect.sizeDelta = new Vector2(800, 200);
        textRect.anchoredPosition = Vector2.zero;

        CreatePlayAgainButton(winScreenObject);

        return winScreenObject;
    }

    private void CreatePlayAgainButton(GameObject winScreenObject)
    {
        GameObject buttonObject = new("PlayAgainButton");
        buttonObject.transform.SetParent(winScreenObject.transform, false);

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
        buttonText.text = "Play Again";
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
