using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using AYellowpaper.SerializedCollections;

public class SoundManager : MonoBehaviour {
    public static SoundManager Instance;

    private void Awake() {
        if (Instance != null) Destroy(gameObject);

        Instance = this;
    }


    [SerializedDictionary("ID", "Audio Clips")]
    [SerializeField] private SerializedDictionary<string, AudioClip[]> soundEffects;

    public static void PlaySound(AudioSource source, string id) {
        AudioClip[] clips;
        if (Instance.soundEffects.TryGetValue(id, out clips)) {
            if (clips.Length == 0) return;
            var clip = clips[Random.Range(0, clips.Length)];
            source.PlayOneShot(clip);
        }
    }
}