using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public AudioClip startupMusic;
    public AudioClip[] music;

    AudioSource audioSource;

//#if !UNITY_EDITOR
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        audioSource.clip = startupMusic;
        audioSource.Play();
    }

    void FixedUpdate()
    {
        if (!audioSource.isPlaying)
        {
            NextTrack();
        }
    }

    void NextTrack()
    {
        AudioClip clip;
        do
        {
            clip = music[Random.Range(0, music.Length - 1)];
        } while (audioSource.clip == clip && music.Length > 1);
        audioSource.clip = clip;
        audioSource.Play();
    }
//#endif 
}
