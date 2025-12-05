using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class DroneHealthParameters
{
    [Tooltip("Health points")]
    public float toughness = 50f; // drone hp
    [HideInInspector]
    public float maxHealth;
}

[System.Serializable]
public class DroneFX
{
    // TODO
    public GameObject damageFX; // GameObject will be instantiated at the point of the hit
    public GameObject deactivateFX; // GameObject will be instantiated at the point of the hit
}

[System.Serializable]
public class DroneAudio
{
    public AudioClip destroyClip; // played when object is destroyed
}

[RequireComponent(typeof(AudioSource))]
public class DroneHealth : MonoBehaviour
{ 
    public DroneHealthParameters parameters;
    public DroneFX VFX;
    public DroneAudio SFX;

    [Header("UI Interface")]
    public Slider healthBar; // TODO: add healthbar to drone

    private void Awake()
    {
        parameters.maxHealth = parameters.toughness;
        UpdateHealthUI();
    }

    public void ReceiveDamage(float damage, Vector3 position)
    {
        if (damage <= parameters.toughness)
        {

            parameters.toughness -= damage;
            UpdateHealthUI();

            if (VFX.damageFX != null)
            {
                GameObject newDamageFX = Instantiate(VFX.damageFX, position, Quaternion.identity);
                Destroy(newDamageFX, 2);
            }

        }
        else
        {
            Die();
        }
    }

    private void Die()
    {
        // explosion when destroyed
        if (VFX.deactivateFX != null)
        {
            GameObject explosion = Instantiate(VFX.deactivateFX, transform.position, Quaternion.identity);
            Destroy(explosion, 3);
        }

        // explosion sound, played independently in world
        if (SFX.destroyClip)
        {
            AudioSource.PlayClipAtPoint(SFX.destroyClip, transform.position, 1.0f);
        }

        Destroy(gameObject);
    }

    private void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.maxValue = parameters.maxHealth;
            healthBar.value = parameters.toughness;
        }
    }
}