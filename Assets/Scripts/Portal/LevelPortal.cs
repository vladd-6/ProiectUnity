using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelPortal : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Next Scene")]
    public string nextSceneName; 

    private void OnTriggerEnter(Collider other)
    {
        // check if player
        if (other.CompareTag("Player"))
        {
            // verifiy if name not null
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.LogError("Null Scene");
            }
        }
    }
}