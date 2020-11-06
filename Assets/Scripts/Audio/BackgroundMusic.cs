using UnityEngine;

public sealed class BackgroundMusic : Singleton<BackgroundMusic>
{
    public AudioClip startupMusic;
    public AudioClip[] music;

    AudioSource audioSource;
    float nextTrackTime;

    public struct Save
    {
        public string musicName;
        public float time;
        public float nextTrackTime;
    }

    protected override void Awake()
    {
        base.Awake();
        Init.Bind += () =>
        {
            audioSource = AudioSystem.instance.GetAudioSource(AudioCategory.BackgroundMusic);
        };
    }

    public void DoUpdate()
    {
        if (!audioSource.isPlaying && GameTime.unscaledTime >= nextTrackTime)
        {
            NextTrack();
        }
    }

    public void GetSave(out Save save)
    {
        AudioClip clip = null;
        if (audioSource && audioSource.isPlaying)
        {
            clip = audioSource.clip;
        }
        save.musicName = clip ? clip.name : "";
        save.time = clip && audioSource ? audioSource.time : 0;
        save.nextTrackTime = nextTrackTime;
    }

    public void SetSave(in Save save)
    {
        nextTrackTime = save.nextTrackTime;
        string clipName = save.musicName;
        AudioClip[] music = this.music;
        for (int i = 0, len = music.Length; i < len; ++i)
        {
            if (clipName == music[i].name)
            {
                //PlayTrack(music[i], save.time);
                return;
            }
        }
        if (!audioSource.isPlaying && Mathf.Approximately(save.nextTrackTime, 0f))
        {
            PlayTrack(startupMusic, 0f);
        }
    }

    void NextTrack()
    {
        AudioClip clip;
        do
        {
            clip = music[Random.Range(0, music.Length - 1)];
        } while (audioSource.clip == clip && music.Length > 1);
        PlayTrack(clip, 0f);
    }

    void PlayTrack(AudioClip clip, float time)
    {
        nextTrackTime = GameTime.unscaledTime + clip.length + Random.Range(10 * 60, 20 * 60);
        audioSource.clip = clip;
        audioSource.time = 0f;
#if UNITY_EDITOR
        if (Mathf.Approximately(GameTime.unscaledTime, 0f))
        {
            return;
        }
#endif
        audioSource.Play();
    }
}
