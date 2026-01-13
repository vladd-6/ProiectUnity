using UnityEngine;

public class LevelMusicSetter : MonoBehaviour
{
    [Header("Music for current level")]
    public AudioClip levelMusic;

    void Start()
    {
        if (MusicManager.instance != null)
        {
            MusicManager.instance.PlayMusic(levelMusic);
        }
    }
}