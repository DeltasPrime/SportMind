using System.Text;
using TMPro;
using UnityEngine;

public class LeaderboardPlayerData : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("TextMeshProUGUI donde se imprimirá el texto. Si se deja vacío, usa el del mismo GameObject.")]
    public TextMeshProUGUI targetText;

    [Header("Auto-Refresh")]
    [Tooltip("Refresca automáticamente al habilitar el objeto.")]
    public bool refreshOnEnable = true;
    [Tooltip("Intervalo en segundos para refresco periódico. 0 = desactivado.")]
    public float refreshInterval = 0f;

    float _timer;

    void Awake()
    {
        if (targetText == null) targetText = GetComponent<TextMeshProUGUI>();
    }

    void OnEnable()
    {
        if (refreshOnEnable) RefreshNow();
        _timer = 0f;
    }

    void Update()
    {
        if (refreshInterval <= 0f) return;
        _timer += Time.deltaTime;
        if (_timer >= refreshInterval)
        {
            _timer = 0f;
            RefreshNow();
        }
    }

    [ContextMenu("Refresh Now")]
    public void RefreshNow()
    {
        if (targetText == null)
        {
            Debug.LogWarning("[Leaderboard] No hay TextMeshProUGUI asignado.");
            return;
        }

        // Seguridad por si no está presente el singleton
        if (PlayerDataStore.Instance == null || PlayerDataStore.Instance.Current == null)
        {
            targetText.text = "No hay datos del jugador.\n(Falta PlayerDataStore en la app)";
            return;
        }

        var p = PlayerDataStore.Instance.Current;

        string S(string v, string fb = "No respondido")
            => string.IsNullOrEmpty(v) ? fb : v;

        var sb = new StringBuilder(512);

        sb.AppendLine($"Nombre: {S(p.playerName)}");
        sb.AppendLine($"Deporte: {S(p.selectedSport)}");
        sb.AppendLine($"Género: {S(p.gender)}");
        sb.AppendLine($"Emocion Inicial: {S(p.emotionalState)}");
        sb.AppendLine();

        sb.AppendLine("--- Tiro al Blanco ---");
        sb.AppendLine($"Puntaje Fácil: {p.shootingScoreEasy}");
        sb.AppendLine($"Pre Emoción Fácil: {S(p.preEmotionTiroEasy)}");
        sb.AppendLine($"Puntaje Difícil: {p.shootingScoreHard}");
        sb.AppendLine($"Pre Emoción Difícil: {S(p.preEmotionTiroHard)}");
        sb.AppendLine($"Emoción Post: {S(p.shootingPostEmotion)}");
        sb.AppendLine($"Rendimiento: {p.shootingRendimiento}");
        sb.AppendLine($"Ritmo Cardíaco: {p.shootingRitmo}");
        sb.AppendLine($"Confianza: {p.shootingConfianza}");
        sb.AppendLine();

        sb.AppendLine("--- Muro de Escalada ---");
        sb.AppendLine($"Tiempo Fácil: {p.climbingTimeEasy:0.##}s");
        sb.AppendLine($"Pre Emoción Fácil: {S(p.preEmotionMuroEasy)}");
        sb.AppendLine($"Tiempo Difícil: {p.climbingTimeHard:0.##}s");
        sb.AppendLine($"Pre Emoción Difícil: {S(p.preEmotionMuroHard)}");
        sb.AppendLine($"Emoción Post: {S(p.climbingPostEmotion)}");
        sb.AppendLine($"Rendimiento: {p.climbingRendimiento}");
        sb.AppendLine($"Ritmo Cardíaco: {p.climbingRitmo}");
        sb.AppendLine($"Confianza: {p.climbingConfianza}");
        sb.AppendLine();

        sb.AppendLine("--- Encuesta Final ---");
        sb.AppendLine($"¿Recomendarías la experiencia?: {p.recomendacionFinal}/10");

        targetText.text = sb.ToString();
    }
}
