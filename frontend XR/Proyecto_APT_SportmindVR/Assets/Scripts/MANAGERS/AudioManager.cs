using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Settings")]
    public AudioClip defaultAmbience;
    [Range(0f, 1f)] public float defaultVolume = 1f;

    private AudioSource track01, track02;
    private bool isPlayingTrack01;
    private float targetVolume = 1f;
    private bool isInitialized = false;
    private AudioClip currentClip;

    public bool IsReady => track01 != null && track02 != null;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeAudio();
    }

    private void InitializeAudio()
    {
        if (isInitialized) return;

        
        track01 = gameObject.AddComponent<AudioSource>();
        track02 = gameObject.AddComponent<AudioSource>();
        track01.loop = true;
        track02.loop = true;

        track01.volume = 0f;
        track02.volume = 0f;
        isPlayingTrack01 = true;

        targetVolume = defaultVolume;
        Play(defaultAmbience, defaultVolume);

        isInitialized = true;
        Debug.Log("AudioManager inicializado.");
    }

    public void SetTargetVolume(float newVolume)
    {
        targetVolume = Mathf.Clamp01(newVolume);
    }

    public void Play(AudioClip newClip, float volume)
    {
        if (newClip == null || newClip == currentClip)
            return;

        SetTargetVolume(volume);
        StopAllCoroutines();
        StartCoroutine(FadeTrack(newClip));
    }

    private IEnumerator FadeTrack(AudioClip newClip)
    {
        float fadeDuration = 1.25f;
        float timeElapsed = 0f;
        currentClip = newClip;

        if (isPlayingTrack01)
        {
            track02.clip = newClip;
            track02.Play();

            float startVolTrack02 = track02.volume;
            float startVolTrack01 = track01.volume;

            while (timeElapsed < fadeDuration)
            {
                float t = timeElapsed / fadeDuration;
                track02.volume = Mathf.Lerp(startVolTrack02, targetVolume, t);
                track01.volume = Mathf.Lerp(startVolTrack01, 0f, t);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            track01.Stop();
            track02.volume = targetVolume;
        }
        else
        {
            track01.clip = newClip;
            track01.Play();

            float startVolTrack01 = track01.volume;
            float startVolTrack02 = track02.volume;

            while (timeElapsed < fadeDuration)
            {
                float t = timeElapsed / fadeDuration;
                track01.volume = Mathf.Lerp(startVolTrack01, targetVolume, t);
                track02.volume = Mathf.Lerp(startVolTrack02, 0f, t);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            track02.Stop();
            track01.volume = targetVolume;
        }

        isPlayingTrack01 = !isPlayingTrack01;
    }
}
