using UnityEngine;

public sealed class BackgroundMusic : Singleton<BackgroundMusic>
{
    public AudioClip startupMusic;
    public AudioClip[] music;

    AudioSource audioSource;

    public struct Save
    {
        public string musicName;
        public float time;
    }

    protected override void Awake()
    {
        base.Awake();
        Init.Bind += () =>
        {
            audioSource = AudioSystem.instance.GetAudioSource(AudioCategory.BackgroundMusic);
            audioSource.clip = startupMusic;
            audioSource.time = 0f;
#if !UNITY_EDITOR
            audioSource.Play();
#endif
        };
    }

    void FixedUpdate()
    {
        if (!audioSource.isPlaying)
        {
            NextTrack();
        }
    }

    public void GetSave(out Save save)
    {
        AudioClip clip = audioSource.clip;
        save.musicName = clip ? clip.name : "";
        save.time = audioSource.time;
    }

    public void SetSave(in Save save)
    {
        string clipName = save.musicName;
        AudioClip[] music = this.music;
        for (int i = 0, len = music.Length; i < len; ++i)
        {
            if (clipName == music[i].name)
            {
                audioSource.clip = music[i];
                audioSource.time = save.time;
#if !UNITY_EDITOR
                audioSource.Play();
#endif
                return;
            }
        }
        NextTrack();
    }

    void NextTrack()
    {
        AudioClip clip;
        do
        {
            clip = music[Random.Range(0, music.Length - 1)];
        } while (audioSource.clip == clip && music.Length > 1);
        audioSource.clip = clip;
        audioSource.time = 0f;
#if !UNITY_EDITOR
        audioSource.Play();
#endif
    }
}
