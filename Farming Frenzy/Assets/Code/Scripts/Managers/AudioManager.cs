using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
   #region Editor Fields
    [Header("Sounds")]
    [SerializeField] private AudioClip[] _musicSounds, _sfxSounds;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
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
        if (sound == null)
        {
            Debug.LogError("Sound Not Found");
            return;
        }
        _musicSource.PlayOneShot(sound);
    }

    public void PlaySFX(string name)
    {
        var sound = Array.Find(_sfxSounds, x => x.name == name);
        if (sound == null)
        {
            Debug.LogError("Sound Not Found");
            return;
        }
        _sfxSource.PlayOneShot(sound);
    }

    public void PlayRandomGoatNoise(){
        int randomNumber = UnityEngine.Random.Range(1, 6);
        string name = "goatNoises" + randomNumber;
        var sound = Array.Find(_sfxSounds, x => x.name == name);
        if (sound == null)
        {
            Debug.LogError("Sound Not Found");
            return;
        }
        _sfxSource.PlayOneShot(sound);
    }
        public void ToggleMusic()
    {
        _musicSource.mute = !_musicSource.mute;
    }

    public void ToggleSFX()
    {
        _sfxSource.mute = !_sfxSource.mute;
    }

    #region SliderSettings

     public void SetMasterVolume(float level) {
        audioMixer.SetFloat("MasterVolume",Mathf.Log10(level)*20f);
    }

    public void SetSoundFXVolume(float level) {
        audioMixer.SetFloat("SoundFXVolume",Mathf.Log10(level)*20f);
    }

    public void SetMusicVolume(float level) {
        audioMixer.SetFloat("MusicVolume",Mathf.Log10(level)*20f);
    }
    #endregion

    #endregion
}
