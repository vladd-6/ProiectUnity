using UnityEngine;

public class HealthBarBillboard : MonoBehaviour
{
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    // lateupdate for updating health bar after player movement
    void LateUpdate()
    {
        if (mainCam != null)
        {
            // update bar to look at camera
            transform.LookAt(transform.position + mainCam.transform.rotation * Vector3.forward,
                             mainCam.transform.rotation * Vector3.up);
        }
    }
}