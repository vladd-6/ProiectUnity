using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DroneWeaponStats
{
    // drone stats (TODO: read them from a file)
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
    public Transform muzzle; // origin for raycast
    public GameObject shotFX; //  GameObject will be instantiated in the shot event
}

[System.Serializable]
public class DroneWeaponAudio
{
    public AudioClip shotClip; // audio played when the drone shoots
}

[RequireComponent(typeof(AudioSource))]
public class DroneWeapon : MonoBehaviour
{
    public DroneWeaponStats stats;
    public DroneWeaponFX VFX;
    public DroneWeaponAudio SFX;

    private float fireTimer;
    private Transform playerTarget;
    private DroneController droneBrain;

    private void Start()
    {
        // find player
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            playerTarget = p.transform;

        // link drone controller
        droneBrain = GetComponent<DroneController>();
    }

    private void Update()
    {
        if (VFX.muzzle == null || droneBrain == null || playerTarget == null)
            return;

        // drone shoots only if in fov (managed in drone controller)
        if (droneBrain.isPlayerVisible)
        {
            // point drone at player
            VFX.muzzle.LookAt(playerTarget.position + Vector3.up);

            // manage cooldown
            fireTimer -= Time.deltaTime;

            if (fireTimer <= 0)
            {
                // final check if in direct sight 
                if (CheckLineOfFire())
                {
                    Shoot();
                    fireTimer = stats.fireRate;
                }
            }
        }
    }

    private bool CheckLineOfFire()
    {
        RaycastHit hit;
        int combinedMask = stats.whatIsEnemy | stats.obstacles;

        // visual debug (TODO: delete)
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
        if (SFX.shotClip)
            GetComponent<AudioSource>().PlayOneShot(SFX.shotClip, Random.Range(0.8f, 1.1f));

        if (VFX.shotFX != null)
        {
            GameObject newShotFX = Instantiate(VFX.shotFX, VFX.muzzle.position, VFX.muzzle.rotation);
            Destroy(newShotFX, 2);
        }

        RaycastHit hit;
        int combinedMask = stats.whatIsEnemy | stats.obstacles;

        if (Physics.SphereCast(VFX.muzzle.position, stats.hitThickness, VFX.muzzle.forward, out hit, stats.range, combinedMask))
        {
            // manage health loss with player script
            HealthController targetActor = hit.collider.GetComponent<HealthController>();
            if (targetActor != null)
            {
                targetActor.ReceiveDamage(stats.power, hit.point);
            }
        }
    }
}