using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// health controller for both enemies and player

[System.Serializable]
public class ActorParameters
{

    [Tooltip("Actor's Health")]
    public float toughness;

    [Tooltip("Threshold of the applied force")]
    public float armor;

    public float damageFactor;
}

[System.Serializable]
public class ActorFX
{

    [Tooltip("Spawn this GameObject at the point of the hit")]
    public GameObject damageFX;

    [Tooltip("Spawn this GameObject when the object is destroyed")]
    public GameObject deactivateFX;
}

[System.Serializable]
public class ActorAudio
{
    public AudioClip destroyClip;
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Collider))]
public class HealthController : MonoBehaviour
{

    [Header("Stats & Effects")]
    public ActorParameters parameters;
    public ActorFX VFX;
    public ActorAudio SFX;

    private Rigidbody actor;

    [Header("UI Interface")]
    public Slider healthBar;

    private float maxHealth;

    void Awake()
    {
        // get references and setup initial health
        actor = GetComponent<Rigidbody>();
        maxHealth = parameters.toughness;
        UpdateHealthUI();
    }

    // updates the slider value with current health
    private void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = parameters.toughness;
        }
    }

    public void ReceiveDamage(float damage, Vector3 position)
    {

        // if actor survives the hit
        if (damage <= parameters.toughness)
        {

            parameters.toughness -= damage;
            UpdateHealthUI();

            // add physical knockback ???
            actor.AddExplosionForce(damage * parameters.damageFactor, position, 0.25f);

            // spawn hit particle effect (TODO)
            if (VFX.damageFX != null)
            {
                GameObject newDamageFX = Instantiate(VFX.damageFX, position, Quaternion.identity);
                Destroy(newDamageFX, 3);
            }

        }
        // actor dies
        else
        {
            parameters.toughness = 0;
            UpdateHealthUI();

            // spawn explosion/death effect
            if (VFX.deactivateFX != null)
            {
                GameObject newDeactivateFX = Instantiate(VFX.deactivateFX, transform.position, Quaternion.identity);
                Destroy(newDeactivateFX, 3);
            }

            // trigger death logic
            Destroy();
        }
    }

    // handle damage from physical collisions (falling, crashing)
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > parameters.armor)
        {
            ReceiveDamage(collision.relativeVelocity.magnitude, transform.position);
        }
    }

    public void Destroy()
    {
        // if the actor is player
        if (gameObject.tag == "Player")
        {
            StartCoroutine(PlayerDeathSequence());
        }
        // if actor is enemy
        else
        {
            // disable physics
            GetComponent<Collider>().enabled = false;

            // hide all visuals
            Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer r in allRenderers)
            {
                r.enabled = false;
            }

            // stop the Weapon script (prevent shooting while dead)
            TurretWeaponController turretScript = GetComponent<TurretWeaponController>();
            if (turretScript != null)
            {
                turretScript.enabled = false;
            }

            // hide the Health Bar
            if (healthBar != null)
            {
                healthBar.gameObject.SetActive(false);
            }

            // play sound and wait before deleting object
            GetComponent<AudioSource>().PlayOneShot(SFX.destroyClip);
            Destroy(gameObject, 2);
        }
    }

    private IEnumerator PlayerDeathSequence()
    {
        // Show cursor so player can click respawn button
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // Disable physics
        GetComponent<Collider>().enabled = false;
        if (TryGetComponent<Rigidbody>(out var rb))
        {
            rb.isKinematic = true;
        }

        // Disable all player control scripts
        MonoBehaviour[] playerScripts = GetComponentsInChildren<MonoBehaviour>();
        foreach (MonoBehaviour script in playerScripts)
        {
            if (script.GetType().Name.Contains("Player") ||
                script.GetType().Name.Contains("Controller") ||
                script.GetType().Name.Contains("Weapon") ||
                script.GetType().Name.Contains("Camera"))
            {
                script.enabled = false;
            }
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
                    deathText.alpha = (fadeProgress - 0.5f) * 2f; // Full opacity by end
                }
            }

            // Fade in button after text, starting at 70% progress
            if (fadeProgress > 0.7f)
            {
                float buttonFadeProgress = (fadeProgress - 0.7f) / 0.3f; // Normalize to 0-1 over last 30%
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

        if (healthBar != null)
        {
            targetCanvas = healthBar.GetComponentInParent<Canvas>();
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
        buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0f); // Start fully transparent

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
        buttonText.color = new Color(1f, 1f, 1f, 0f); // Start fully transparent

        RectTransform buttonTextRect = buttonTextObject.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;

        button.onClick.AddListener(() => SceneManager.LoadScene(SceneManager.GetActiveScene().name));
    }
}