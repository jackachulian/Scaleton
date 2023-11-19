using UnityEngine;
using UnityEngine.Audio;

public class SettingsMenu : Menu {

    [SerializeField] private AudioMixer mixer;
    [SerializeField] private string mixerParam = "sfx";
    public void SetVolume(float volume) {
        Debug.Log(volume);
        mixer.SetFloat(mixerParam, volume);
    }
}