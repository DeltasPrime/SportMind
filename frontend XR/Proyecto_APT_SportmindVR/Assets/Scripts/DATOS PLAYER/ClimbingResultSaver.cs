using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClimbingResultsSaver : MonoBehaviour
{
    [Header("Grupos de Toggles")]
    [SerializeField] private ToggleGroup postEmotionGroup;
    [SerializeField] private Toggle[] postEmotionToggles; // texto (Calma/Feliz/etc.)

    [SerializeField] private ToggleGroup rendimientoGroup;
    [SerializeField] private Toggle[] rendimientoToggles; // números 1..5

    [SerializeField] private ToggleGroup ritmoGroup;
    [SerializeField] private Toggle[] ritmoToggles;       // números 1..5

    [SerializeField] private ToggleGroup confianzaGroup;
    [SerializeField] private Toggle[] confianzaToggles;   // números 1..5

    // Llama este método desde tu botón "Guardar / Siguiente"
    public void SaveAll()
    {
        if (PlayerDataStore.Instance == null) return;
        if (PlayerDataStore.Instance.Current == null)
            PlayerDataStore.Instance.Current = new PlayerData(); // crea si faltara

        var data = PlayerDataStore.Instance.Current;

        // 1) Post-Emotion (string)
        string postEmotion = GetSelectedText(postEmotionToggles);
        if (!string.IsNullOrEmpty(postEmotion))
            data.climbingPostEmotion = postEmotion; // string

        // 2) Rendimiento (int)
        int? rend = GetSelectedInt(rendimientoToggles);
        if (rend.HasValue)
            data.climbingRendimiento = rend.Value;

        // 3) Ritmo (int)
        int? ritmo = GetSelectedInt(ritmoToggles);
        if (ritmo.HasValue)
            data.climbingRitmo = ritmo.Value;

        // 4) Confianza (int)
        int? conf = GetSelectedInt(confianzaToggles);
        if (conf.HasValue)
            data.climbingConfianza = conf.Value;

        // (Opcional: Debug)
        // Debug.Log("Climbing data guardada:\n" + PlayerDataStore.Instance.ExportToJson());
    }

    // --- Helpers mínimos ---
    private string GetSelectedText(Toggle[] toggles)
    {
        if (toggles == null) return null;
        foreach (var t in toggles)
        {
            if (t != null && t.isOn)
            {
                // tomar el primer TMP_Text hijo como etiqueta
                var label = t.GetComponentInChildren<TMP_Text>();
                return (label && !string.IsNullOrWhiteSpace(label.text))
                    ? label.text.Trim()
                    : t.gameObject.name.Trim();
            }
        }
        return null; // nada seleccionado
    }

    private int? GetSelectedInt(Toggle[] toggles)
    {
        string txt = GetSelectedText(toggles);
        if (string.IsNullOrEmpty(txt)) return null;

        if (int.TryParse(txt, out int value)) return value;

        // Si los toggles no son números puros, aquí puedes mapear manualmente:
        // p.ej., if (txt.Equals("Alto")) return 5; etc.
        Debug.LogWarning("El texto seleccionado no es un número válido: " + txt);
        return null;
    }
}
