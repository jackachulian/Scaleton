using UnityEngine;
using UnityEngine.Audio;

public class SettingsMenu : Menu {

    [SerializeField] private AudioMixer mixer;

    public void SetSFXVolume(float volume) {
        Debug.Log(volume);
        mixer.SetFloat("sfx", volume);
    }

    public void SetMusicVolume(float volume) {
        Debug.Log(volume);
        mixer.SetFloat("music", volume);
    }

    public void SetFullscreen(bool isFullscreen) {
        Screen.fullScreen = isFullscreen;
    }
}