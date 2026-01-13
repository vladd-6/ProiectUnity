using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager instance;
    private AudioSource audioSource;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayMusic(AudioClip newClip)
    {
        if (audioSource == null) return;

        if (audioSource.clip == newClip && audioSource.isPlaying) return;

        audioSource.Stop();
        audioSource.clip = newClip;
        audioSource.Play();
    }
}