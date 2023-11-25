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
        if (!source.enabled) return;
        if (!source.gameObject.activeInHierarchy) return;
        
        AudioClip clip = GetClip(id);
        if (clip) {
            source.PlayOneShot(clip);
        }
    }

    public static void PlaySound(Vector3 position, string id) {
        AudioClip clip = GetClip(id);
        if (clip) {
            AudioSource.PlayClipAtPoint(clip, position);
            Debug.Log(clip+" played");
        }
    }

    public static AudioClip GetClip(string id) {
        AudioClip[] clips;
        if (Instance.soundEffects.TryGetValue(id, out clips)) {
            if (clips.Length == 0) return null;
            return clips[Random.Range(0, clips.Length)];
        } else {
            Debug.LogError("No sound with ID "+id+" found");
        }
        return null;
    }
}