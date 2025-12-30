using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
            Debug.Log("GAME OVER! Player died.");
            // TODO player logic
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

    public bool Heal(float amount)
    {
        // check if already full health 
        if (parameters.toughness >= maxHealth)
        {
            return false;
        }

        // add heath
        parameters.toughness += amount;

        if (parameters.toughness > maxHealth)
        {
            parameters.toughness = maxHealth;
        }

        // update
        UpdateHealthUI();

        return true;
    }
}