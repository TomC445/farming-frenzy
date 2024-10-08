using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class AudioSettings : MonoBehaviour
{
    #region Editor Fields
    [SerializeField] private AudioMixer audioMixer;
    #endregion

    #region Properties

    #endregion

    #region Methods
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
}
