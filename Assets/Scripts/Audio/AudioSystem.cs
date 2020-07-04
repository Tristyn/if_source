using System;
using UnityEngine;

public enum AudioCategory
{
    Effect,
    Ambience,
    BackgroundMusic,
}

public class AudioSystem : Singleton<AudioSystem>
{
    public AudioSource effectAudioSource;
    public AudioSource ambienceAudioSource;
    public AudioSource backgroundMusicAudioSource;

    public void PlayOneShot(AudioClip clip, AudioCategory category)
    {
        if (clip)
        {
            AudioSource audioSource = GetAudioSource(category);
            audioSource.PlayOneShot(clip);
        }
    }

    AudioSource GetAudioSource(AudioCategory category)
    {
        switch (category)
        {
            case AudioCategory.Effect:
                return effectAudioSource;
            case AudioCategory.Ambience:
                return ambienceAudioSource;
            case AudioCategory.BackgroundMusic:
                return backgroundMusicAudioSource;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
