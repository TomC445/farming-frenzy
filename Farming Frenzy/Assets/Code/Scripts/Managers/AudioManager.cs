using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    #region Editor Fields
    [Header("Sounds")]
    [SerializeField] private Sound[] _musicSounds, _sfxSounds;
    [Header("Audio Sources")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;
    #endregion

    #region Properties

    #endregion

    #region Singleton
    public static AudioManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

    }
    #endregion

    #region Methods
    public void PlayMusic(string name)
    {
        var sound = Array.Find(_musicSounds, x => x.name == name);
        if (sound != null)
        {
            Debug.LogError("Sound Not Found");
            return;
        }
        _musicSource.clip = sound.AudioClip;
        _musicSource.Play();
    }

    public void PlaySFX(string name)
    {
        var sound = Array.Find(_sfxSounds, x => x.name == name);
        if (sound != null)
        {
            Debug.LogError("Sound Not Found");
            return;
        }
        _musicSource.PlayOneShot(sound.AudioClip);
    }

    public void ToggleMusic()
    {
        _musicSource.mute = !_musicSource.mute;
    }

    public void ToggleSFX()
    {
        _sfxSource.mute = !_sfxSource.mute;
    }

    public void MusicVolume(float volume)
    {
        _musicSource.volume = volume;
    }

    public void SFXVolume(float volume)
    {
        _sfxSource.volume = volume;
    }
    #endregion
}
