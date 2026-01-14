using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    public AudioMixer audioMixer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetMasterVolumeFromSlider(float sliderValue)
    {
        float volumeDb = Mathf.Lerp(-80f, 0f, sliderValue);
        Debug.Log("Volume:" + volumeDb.ToString());
        SetMasterVolume(volumeDb);
    }

    // Set global volume (dB: -80 = mute, 0 = full)
    public void SetMasterVolume(float volumeDb)
    {
        audioMixer.SetFloat("MyExposedParam", volumeDb);
    }

    // Get current global volume (dB)
    public float GetMasterVolume()
    {
        audioMixer.GetFloat("MyExposedParam", out float volumeDb);
        return volumeDb;
    }
}
