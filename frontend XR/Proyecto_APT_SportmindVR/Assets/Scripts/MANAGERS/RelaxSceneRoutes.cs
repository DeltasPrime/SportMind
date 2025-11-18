using System.Collections;
using UnityEngine;

public class FinalSceneRoutes : MonoBehaviour
{
    [Header("Objetivos")]
    [Tooltip("Nombre exacto del objetivo de Relax.")]
    public string relaxObjectiveName = "Relax_Completada";

    [Header("Audio")]
    [Tooltip("Audio para la ruta Relax (se usa en casos 2 y 3).")]
    public AudioClip relaxRouteClip;
    [Tooltip("Audio para la ruta de despedida cuando TODO está completado (caso 1).")]
    public AudioClip farewellClip;
    [Tooltip("AudioSource opcional. Si no asignas, se crea uno en runtime.")]
    public AudioSource audioSource;

    [Header("UI / Objetos")]
    [Tooltip("Canvas que aparece para cambiar de escena en los casos 2 y/o 3 según corresponda.")]
    public GameObject changeSceneCanvas;
    [Tooltip("Canvas de despedida cuando TODO está completado (caso 1).")]
    public GameObject farewellCanvas;
    [Tooltip("GameObject extra de despedida cuando TODO está completado (caso 1).")]
    public GameObject farewellObject;
    [Tooltip("GameObject de la escena que debe desactivarse cuando TODO está completado (caso 1).")]
    public GameObject objectToDisableOnFarewell;

    [Header("Ejecución")]
    public bool autoRunOnStart = true;

    // Internos
    private bool _ran;

    private void Awake()
    {
        if (!audioSource)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void Start()
    {
        if (autoRunOnStart && !_ran) StartCoroutine(RunLogicOnce());
    }

    /// <summary>
    /// Si prefieres dispararlo manualmente (por botón/evento), llama a este método.
    /// </summary>
    public void ExecuteNow()
    {
        if (!_ran) StartCoroutine(RunLogicOnce());
    }

    private IEnumerator RunLogicOnce()
    {
        _ran = true;

        // Espera a que ProgressManager esté listo (si proviene de otra escena)
        for (float t = 0; t < 1.0f && ProgressManager.Instance == null; t += Time.unscaledDeltaTime)
            yield return null;

        bool hasPM = ProgressManager.Instance != null;

        bool allCompleted = hasPM && ProgressManager.Instance.CheckAllObjectivesCompleted();
        bool relaxCompleted = hasPM && ProgressManager.Instance.IsObjectiveCompleted(relaxObjectiveName);

        // === CASO 1: TODO completado ===
        // - Aparece el canvas de despedida
        // - Se desactiva un GameObject de la escena
        // - Se activa el GameObject de despedida
        // - TODO esto MIENTRAS se reproduce el audio de despedida
        if (allCompleted)
        {
            if (farewellCanvas) farewellCanvas.SetActive(true);
            if (objectToDisableOnFarewell) objectToDisableOnFarewell.SetActive(false);
            if (farewellObject) farewellObject.SetActive(true);

            yield return PlayOneShotIfAny(farewellClip); // reproduce mientras lo anterior ya está activo
            yield break;
        }

        // === CASO 2: Relax_Completada presente (pero no todo completado) ===
        // - Se reproduce el audio de ruta Relax
        // - AL MISMO TIEMPO aparece el canvas de cambio de escena
        if (relaxCompleted)
        {
            if (changeSceneCanvas) changeSceneCanvas.SetActive(true); // aparece durante el audio
            yield return PlayOneShotIfAny(relaxRouteClip);
            yield break;
        }

        // === CASO 3: NO está Relax_Completada ===
        // - Se reproduce el audio de ruta Relax
        // - AL TERMINAR el audio, se activa el canvas de cambio de escena
        yield return PlayOneShotIfAny(relaxRouteClip);
        if (changeSceneCanvas) changeSceneCanvas.SetActive(true);
    }

    private IEnumerator PlayOneShotIfAny(AudioClip clip)
    {
        if (clip == null) yield break;

        // Como PlayOneShot no asigna .clip, usamos una corrutina temporal para medir la duración
        audioSource.PlayOneShot(clip);

        // Espera el tiempo de duración del clip si está disponible, en fallback usa isPlaying
        float duration = clip.length > 0f ? clip.length : 0f;
        if (duration > 0f)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                yield return null;
            }
        }
        else
        {
            while (audioSource.isPlaying)
                yield return null;
        }
    }
}
