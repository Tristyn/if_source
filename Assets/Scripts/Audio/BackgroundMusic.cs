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

    public void DoUpdate()
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
                PlayTrack(music[i], save.time);
                return;
            }
        }
        if (!audioSource.isPlaying)
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
        audioSource.clip = clip;
        audioSource.time = 0f;
#if !UNITY_EDITOR
        audioSource.Play();
#endif
    }
}
