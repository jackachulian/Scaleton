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

    [SerializeField] private float musicFadeTime = 10f;

    [SerializeField] private AudioSource fadingToSource, fadingFromSource;
    // 0 - fully source 1 playing, 1 - fully source 2 playing
    float musicFadeValue;
    bool fading;

    public void PlayMusic(AudioClip clip, bool fade = true) {
        if (fadingToSource.clip == clip) return;

        AudioSource temp = fadingFromSource;
        fadingFromSource = fadingToSource;
        fadingToSource = temp;

        fadingToSource.clip = clip;

        if (fade) {
            musicFadeValue = 0;
            fadingToSource.volume = 0;
            fading = true;
        } else {
            fadingToSource.volume = 1f;
            fadingFromSource.Stop();
        }

        fadingToSource.Play();
    }

    public void StopMusic() {
        fadingToSource.Stop();
        fadingFromSource.Stop();
    }

    private void Update() {
        if (fading) {
            musicFadeValue += Time.deltaTime / musicFadeTime;
            if (musicFadeValue >= 1) {
                musicFadeValue = 1;
                fading = false;
            }
            fadingToSource.volume = musicFadeValue;
            fadingFromSource.volume = 1 - musicFadeValue;
        }
    }

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