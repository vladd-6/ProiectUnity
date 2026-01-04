using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Set?ri")]
    public float healAmount = 25f;
    public float rotateSpeed = 50f;

    void Update()
    {
        transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HealthController healthScript = other.GetComponent<HealthController>();

            if (healthScript != null)
            {
                if (healthScript.Heal(healAmount))
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}