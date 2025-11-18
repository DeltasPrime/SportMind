using UnityEngine;

public class SceneEntryAudio : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip audioBienvenida;
    public AudioClip audioRegreso;

    private static int vecesEntrado = 0; // Siempre se reinicia al cerrar el juego

    private void Start()
    {
        vecesEntrado++;

        if (vecesEntrado == 1)
        {
            audioSource.clip = audioBienvenida;
            audioSource.Play();
        }
        else if (vecesEntrado == 2)
        {
            audioSource.clip = audioRegreso;
            audioSource.Play();
        }
    }
}