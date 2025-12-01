using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class TurretParameters
{

    [Header("Status")]
    [Tooltip("Activate or deactivate the Turret")]
    public bool active;
    public bool canFire;

    [Header("Shooting")]
    [Tooltip("Burst the force when hit")]
    public float power;

    [Tooltip("Pause between shooting")]
    [Range(0.5f, 2)]
    public float ShootingDelay;

    [Tooltip("Radius of the turret view")]
    public float radius;
}

[System.Serializable]
public class TurretFX
{

    [Tooltip("Muzzle transform position")]
    public Transform muzzle;

    [Tooltip("Spawn this GameObject when shooting")]
    public GameObject shotFX;
}

[System.Serializable]
public class TurretAudio
{
    public AudioClip shotClip;
}

[System.Serializable]
public class TurretTargeting
{

    [Tooltip("Speed of aiming at the target")]
    public float aimingSpeed;

    [Tooltip("Pause before the aiming")]
    public float aimingDelay;

    [Tooltip("GameObject with folowing tags will be identify as enemy")]
    public string[] tagsToFire;

    public List<Collider> targets = new List<Collider>();
    public Collider target;
}

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(HealthController))]
[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Animator))]
public class TurretWeaponController : MonoBehaviour
{

    [Header("Settings")]
    public TurretParameters parameters;
    public TurretTargeting targeting;
    public TurretFX VFX;
    public TurretAudio SFX;

    private float fireTimer;

    private void Awake()
    {
        // setup the detection range (sphere collider)
        GetComponent<SphereCollider>().isTrigger = true;
        GetComponent<SphereCollider>().radius = parameters.radius;

        // setup the physical body (box collider)
        GetComponent<BoxCollider>().size = new Vector3(2, 2, 2);
        GetComponent<BoxCollider>().center = new Vector3(0, 1, 0);
    }

    private void Update()
    {
        // check if turret is active
        if (parameters.active == false)
        {
            return;
        }

        // cleanup invalid targets
        if (targeting.target == null)
        {
            ClearTargets();
            return;
        }

        // handle fire cooldown
        fireTimer -= Time.deltaTime;

        // combat logic
        if (targeting.target != null && CanSeeTarget())
        {

            // rotate towards enemy
            Aiming();

            // shoot if cooldown is ready
            if (fireTimer <= 0)
            {
                Shooting();
                fireTimer = parameters.ShootingDelay;
            }
        }
    }

    // check if there are walls between turret and player
    private bool CanSeeTarget()
    {
        if (targeting.target == null)
            return false;

        // compute direction and distance
        Vector3 dirToTarget = (targeting.target.bounds.center - VFX.muzzle.position).normalized;
        float distToTarget = Vector3.Distance(VFX.muzzle.position, targeting.target.bounds.center);

        RaycastHit hit;
        if (Physics.Raycast(VFX.muzzle.position, dirToTarget, out hit, parameters.radius))
        {
            // check if we hit the target / something with the right tag
            bool isTarget = (hit.collider == targeting.target || CheckTags(hit.collider));

            // for debug (green = visible, red = blocked) (TODO: remove)
            Debug.DrawLine(VFX.muzzle.position, hit.point, isTarget ? Color.green : Color.red);

            if (isTarget)
            {
                return true;
            }
        }

        return false;
    }

    #region Aiming and Shooting

    // handle visuals and audio
    private void Shot()
    {
        GetComponent<AudioSource>().PlayOneShot(SFX.shotClip, Random.Range(0.75f, 1));

        if (VFX.shotFX != null)
        {
            GetComponent<Animator>().SetTrigger("Shot");

            // spawn muzzle flash
            GameObject newShotFX = Instantiate(VFX.shotFX, VFX.muzzle);
            Destroy(newShotFX, 2);
        }
    }

    // handle logic and damage
    private void Shooting()
    {

        if (targeting.target == null) return;
        if (parameters.canFire == false) return;

        // play effects
        Shot();

        Vector3 dirToTarget = (targeting.target.bounds.center - VFX.muzzle.position).normalized;
        float distToTarget = Vector3.Distance(VFX.muzzle.position, targeting.target.bounds.center);

        RaycastHit hit;
        if (Physics.Raycast(VFX.muzzle.position, dirToTarget, out hit, parameters.radius))
        {
            // verify tag 
            if (CheckTags(hit.collider) == true)
            {

                // apply damage to the HealthController script
                HealthController targetActor = hit.collider.GetComponent<HealthController>();
                if (targetActor != null)
                {
                    targetActor.ReceiveDamage(parameters.power, hit.point);
                }
            }
        }
    }

    // rotate the turret 
    public void Aiming()
    {

        if (targeting.target == null) return;

        Vector3 direction = targeting.target.transform.position - transform.position;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, targeting.aimingSpeed * Time.deltaTime);
        }
    }

    #endregion

    #region Targeting (Radar System)

    // player enters the detection radius
    private void OnTriggerEnter(Collider other)
    {

        if (parameters.active == false) return;

        ClearTargets();

        // add to the list the valid enemy
        if (CheckTags(other) == true)
        {
            if (targeting.targets.Count == 0)
            {
                targeting.target = other.GetComponent<Collider>();
            }

            targeting.targets.Add(other.GetComponent<Collider>());
        }
    }

    // enemy leaves the detection radius
    private void OnTriggerExit(Collider other)
    {

        if (parameters.active == false) return;

        ClearTargets();

        // remove from list
        if (CheckTags(other) == true)
        {
            targeting.targets.Remove(other.GetComponent<Collider>());

            // select next target if available
            if (targeting.targets.Count != 0)
            {
                targeting.target = targeting.targets.First();
            }
            else
            {
                targeting.target = null;
            }
        }
    }

    // helper to compare object tags
    private bool CheckTags(Collider toMatch)
    {

        bool Match = false;
        for (int i = 0; i < targeting.tagsToFire.Length; i++)
        {
            if (toMatch.tag == targeting.tagsToFire[i])
            {
                Match = true;
            }
        }
        return (Match);
    }

    // clean up lists (remove dead/destroyed objects)
    private void ClearTargets()
    {

        if (targeting.target != null)
        {
            if (targeting.target.GetComponent<Collider>().enabled == false)
            {
                targeting.targets.Remove(targeting.target);
            }
        }

        foreach (Collider target in targeting.targets.ToList())
        {

            if (target == null)
            {
                targeting.targets.Remove(target);
            }

            if (targeting.targets.Count != 0)
            {
                targeting.target = targeting.targets.First();
            }
            else
            {
                targeting.target = null;
            }
        }
    }

    #endregion
}