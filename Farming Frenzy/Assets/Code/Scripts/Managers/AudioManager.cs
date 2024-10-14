using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

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
    private Slider _masterSlider;
    private Slider _bgmSlider;
    private Slider _sfxSlider;
    private float _masterValue;
    private float _bgmValue;
    private float _sfxValue;
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

    void Start()
    {
        SetInitialMusicVolume();
        _masterValue = -1;
        _bgmValue = -1;
        _sfxValue = -1;
    }

    void Update()
    {
        GameObject[] sliders = GameObject.FindGameObjectsWithTag("slider");
        if(sliders.Length > 0) {
            _masterSlider = sliders[0].GetComponent<Slider>();
            _bgmSlider = sliders[1].GetComponent<Slider>();
            _sfxSlider = sliders[2].GetComponent<Slider>();
            if(_masterValue != -1) _masterSlider.value = _masterValue;
            if(_bgmValue != -1) _bgmSlider.value = _bgmValue;
            if(_sfxValue != -1) _sfxSlider.value = _sfxValue;
            SetSliders();
        }
    }


    #region Methods
    public void SetSliders() {
        _masterSlider.onValueChanged.AddListener(SetMasterVolume);
        _bgmSlider.onValueChanged.AddListener(SetMusicVolume);
        _sfxSlider.onValueChanged.AddListener(SetSoundFXVolume);
    }
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

    public void RestartMusic() {
        _musicSource.Stop();
        _musicSource.Play();
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

    public void SetInitialMusicVolume() {
        //RestartMusic();
        _musicSource.mute = false;
        _sfxSource.mute = false;
        _sfxSource.Stop();
        audioMixer.SetFloat("MusicVolume",Mathf.Log10(0.5f)*20f);
    }

    public void ToggleSFX()
    {
        if(!_sfxSource.mute) _sfxSource.Stop();
        _sfxSource.mute = !_sfxSource.mute;
    }

    #region SliderSettings

     public void SetMasterVolume(float level) {
        audioMixer.SetFloat("MasterVolume",Mathf.Log10(level)*20f);
        _masterValue = level;
    }

    public void SetSoundFXVolume(float level) {
        audioMixer.SetFloat("SoundFXVolume",Mathf.Log10(level)*20f);
        _sfxValue = level;
    }

    public void SetMusicVolume(float level) {
        audioMixer.SetFloat("MusicVolume",Mathf.Log10(level)*20f);
        _bgmValue = level;
    }
    #endregion

    #endregion
}
