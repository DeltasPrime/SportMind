using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioSwap : MonoBehaviour
{
    [Header("Escena especial")]
    public int targetSceneIndex = 4;
    public AudioClip specialTrack;
    [Range(0f, 1f)] public float specialVolume = 1f;

    private AudioManager audioManager;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        audioManager = AudioManager.instance;
        
        Scene current = SceneManager.GetActiveScene();
        HandleScene(current.buildIndex);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        HandleScene(scene.buildIndex);
    }

    private void HandleScene(int buildIndex)
    {
        if (audioManager == null || specialTrack == null)
            return;

        if (buildIndex == targetSceneIndex)
        {
            audioManager.Play(specialTrack, specialVolume);
        }
        else
        {
            audioManager.Play(audioManager.defaultAmbience, audioManager.defaultVolume);
        }
    }
}


