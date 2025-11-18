using UnityEngine;

public class GoalTrigger : MonoBehaviour
{
    [Header("Detección")]
    [Tooltip("Tag del jugador (XROrigin).")]
    public string playerTag = "Player";

    [Header("Feedback")]
    [Tooltip("Audio a reproducir al entrar. Opcional.")]
    public AudioSource goalAudio;

    [Tooltip("GameObject de partículas (si está desactivado se activará). Opcional.")]
    public GameObject particleGO;

    [Tooltip("ParticleSystem (se le hará Play). Opcional.")]
    public ParticleSystem particleFX;

    [Header("Comportamiento")]
    [Tooltip("Evitar múltiples activaciones deshabilitando el collider tras el trigger.")]
    public bool disableColliderAfterTrigger = true;

    private bool hasTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag(playerTag)) return;

        hasTriggered = true;

        // 1) Audio
        if (goalAudio != null)
        {
            // Si el GO del audio está desactivado en escena, actívalo primero
            if (!goalAudio.gameObject.activeSelf)
                goalAudio.gameObject.SetActive(true);

            // Reproduce (usa PlayOneShot si prefieres no interrumpir otros audios del mismo source)
            goalAudio.Play();
            // goalAudio.PlayOneShot(goalAudio.clip); // alternativa
        }

        // 2) Partículas como GameObject
        if (particleGO != null)
        {
            if (!particleGO.activeSelf)
                particleGO.SetActive(true);
        }

        // 3) Partículas como ParticleSystem
        if (particleFX != null)
        {
            if (!particleFX.gameObject.activeSelf)
                particleFX.gameObject.SetActive(true);

            particleFX.Play(true);
        }

        // 4) Lógica del juego existente
        if (ClimbingGameManager.Instance != null)
            ClimbingGameManager.Instance.EndByGoal();

        // 5) Evitar retriggers
        if (disableColliderAfterTrigger)
        {
            var col = GetComponent<Collider>();
            if (col) col.enabled = false;
        }
    }
}
