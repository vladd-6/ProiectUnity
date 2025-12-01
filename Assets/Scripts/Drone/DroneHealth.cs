using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Am redenumit clasele ajutatoare
[System.Serializable]
public class DroneHealthParameters
{
    [Tooltip("Health points")]
    public float toughness = 50f;
    [HideInInspector]
    public float maxHealth;
}

[System.Serializable]
public class DroneFX
{
    public GameObject damageFX;
    public GameObject deactivateFX; // Explozia finala
}

[System.Serializable]
public class DroneAudio
{
    public AudioClip destroyClip;
}

[RequireComponent(typeof(AudioSource))]
public class DroneHealth : MonoBehaviour
{ // Nume clasa nou

    public DroneHealthParameters parameters;
    public DroneFX VFX;
    public DroneAudio SFX;

    [Header("UI Interface")]
    public Slider healthBar;

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
        // 1. Efecte Vizuale (Explozie)
        if (VFX.deactivateFX != null)
        {
            GameObject explosion = Instantiate(VFX.deactivateFX, transform.position, Quaternion.identity);
            Destroy(explosion, 3);
        }

        // 2. SUNETUL (Solutia Eleganta)
        // Cream un sunet independent in lume, la pozitia dronei
        if (SFX.destroyClip)
        {
            // Parametri: Clipul, Pozitia, Volumul (1.0f = 100%)
            AudioSource.PlayClipAtPoint(SFX.destroyClip, transform.position, 1.0f);
        }

        // 3. LOGICA DE JOC (Doar pentru Player)
        if (gameObject.tag == "Player")
        {
            Debug.Log("GAME OVER");
            // Aici nu dam Destroy, doar dezactivam controalele
            // logic? de game over...
        }
        else
        {
            // 4. DISTRUGERE INSTANTANEE (Pentru Drona)
            // Nu mai e nevoie sa dezactivam manual rendere, lumini, colidere.
            // Stergem tot obiectul acum.
            Destroy(gameObject);
        }
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