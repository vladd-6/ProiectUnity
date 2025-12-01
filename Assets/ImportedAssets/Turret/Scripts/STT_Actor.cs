using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ActorParameters {
	
	[Tooltip("Actor's Health")]
	public float toughness;
	[Tooltip("Threshold of the applied force")]
	public float armor;
	public float damageFactor;
}
	
[System.Serializable]
public class ActorFX {
	
	[Tooltip("Spawn this GameObject when the turrent is hitting")]
	public GameObject damageFX;
	[Tooltip("Spawn this GameObject when the turrent is destroyed")]
	public GameObject deactivateFX;
}

[System.Serializable]
public class ActorAudio {

	public AudioClip destroyClip;
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AudioSource))]
public class STT_Actor : MonoBehaviour {

	public ActorParameters parameters;
	public ActorFX VFX;
	public ActorAudio SFX;

	private Rigidbody actor;

    [Header("UI Interface")]
    public Slider healthBar;

    private float maxHealth;

    void Awake()
	{
		actor = GetComponent<Rigidbody> ();
        maxHealth = parameters.toughness;
        UpdateHealthUI();
    }

    private void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = parameters.toughness;
        }
    }

    public void ReceiveDamage(float damage, Vector3 position) {

        if (damage <= parameters.toughness) {
			
			parameters.toughness -= damage;
            UpdateHealthUI();
            actor.AddExplosionForce (damage * parameters.damageFactor, position, 0.25f);

			if (VFX.damageFX != null) {
				GameObject newDamageFX = Instantiate (VFX.damageFX, position, Quaternion.identity);
				Destroy (newDamageFX, 3);
			}

		} else {
            parameters.toughness = 0;
            UpdateHealthUI();

            if (VFX.deactivateFX != null) {
				GameObject newDeactivateFX = Instantiate (VFX.deactivateFX, transform.position, Quaternion.identity);
				Destroy (newDeactivateFX, 3);
			}

			Destroy ();
		}
	}

	private void OnCollisionEnter(Collision collision) {

		if (collision.relativeVelocity.magnitude > parameters.armor) {
			ReceiveDamage (collision.relativeVelocity.magnitude, transform.position);
		}
	}

	public void Destroy() {
        if (gameObject.tag == "Player")
        {
            Debug.Log("GAME OVER! Player-ul a murit.");
        }
        else
        {
            GetComponent<Collider>().enabled = false;
            Renderer[] allRenderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer r in allRenderers)
            {
                r.enabled = false;
            }

            STT_Turret turretScript = GetComponent<STT_Turret>();
            if (turretScript != null)
            {
                turretScript.enabled = false; 
            }

            GetComponent<AudioSource>().PlayOneShot(SFX.destroyClip);
            Destroy(gameObject, 2);
        }
    }
}
