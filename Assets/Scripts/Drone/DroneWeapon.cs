using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DroneWeaponStats
{
    [Header("Combat Stats")]
    public float power = 10f;
    public float fireRate = 0.5f;
    public float range = 30f;
    public float hitThickness = 0.5f;

    [Header("Detection")]
    public LayerMask whatIsEnemy;
    public LayerMask obstacles;
}

[System.Serializable]
public class DroneWeaponFX
{
    public Transform muzzle;
    public GameObject shotFX;
}

[System.Serializable]
public class DroneWeaponAudio
{
    public AudioClip shotClip;
}

[RequireComponent(typeof(AudioSource))]
public class DroneWeapon : MonoBehaviour
{

    public DroneWeaponStats stats;
    public DroneWeaponFX VFX;
    public DroneWeaponAudio SFX;

    private float fireTimer;
    private Transform playerTarget;
    private DroneController droneBrain; // Referinta la scriptul de miscare

    private void Start()
    {
        // 1. Gasim Player-ul
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTarget = p.transform;

        // 2. Ne conectam la "Creierul" dronei (DroneController) de pe acelasi obiect
        droneBrain = GetComponent<DroneController>();
    }

    private void Update()
    {
        if (VFX.muzzle == null || droneBrain == null || playerTarget == null) return;

        // --- MODIFICAREA PRINCIPALA ---
        // Arma functioneaza DOAR daca Drona a decis ca te vede (Lumina Rosie)
        if (droneBrain.isPlayerVisible)
        {
            // 1. Orientam teava spre pieptul player-ului
            VFX.muzzle.LookAt(playerTarget.position + Vector3.up * 1.5f);

            // 2. Gestionam tragerea
            fireTimer -= Time.deltaTime;

            if (fireTimer <= 0)
            {
                // Facem totusi verificarea finala daca linia de tragere e libera
                if (CheckLineOfFire())
                {
                    Shoot();
                    fireTimer = stats.fireRate;
                }
            }
        }
    }

    // Am redenumit functia ca sa fie mai clar ce face (Verifica linia de tragere)
    private bool CheckLineOfFire()
    {
        RaycastHit hit;
        int combinedMask = stats.whatIsEnemy | stats.obstacles;

        // Debug vizual: Verde cand trage
        Debug.DrawRay(VFX.muzzle.position, VFX.muzzle.forward * stats.range, Color.green);

        if (Physics.SphereCast(VFX.muzzle.position, stats.hitThickness, VFX.muzzle.forward, out hit, stats.range, combinedMask))
        {
            if (((1 << hit.collider.gameObject.layer) & stats.whatIsEnemy) != 0)
            {
                return true;
            }
        }
        return false;
    }

    private void Shoot()
    {
        if (SFX.shotClip) GetComponent<AudioSource>().PlayOneShot(SFX.shotClip, Random.Range(0.8f, 1.1f));

        if (VFX.shotFX != null)
        {
            GameObject newShotFX = Instantiate(VFX.shotFX, VFX.muzzle.position, VFX.muzzle.rotation);
            Destroy(newShotFX, 2);
        }

        RaycastHit hit;
        int combinedMask = stats.whatIsEnemy | stats.obstacles;

        if (Physics.SphereCast(VFX.muzzle.position, stats.hitThickness, VFX.muzzle.forward, out hit, stats.range, combinedMask))
        {
            STT_Actor targetActor = hit.collider.GetComponent<STT_Actor>();
            if (targetActor != null)
            {
                targetActor.ReceiveDamage(stats.power, hit.point);
            }
        }
    }
}