using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EmotionSaver : MonoBehaviour
{
    [SerializeField] private ToggleGroup emotionsGroup; // grupo con 6 toggles

    // Llama este método desde tu botón "Siguiente/Guardar"
    public void SaveEmotion()
    {
        if (emotionsGroup == null) return;

        var active = emotionsGroup.GetFirstActiveToggle();
        if (active == null) return; // si no hay ninguno seleccionado, no hace nada

        // Obtiene el texto del toggle (si no hay label, usa el nombre del GameObject)
        string value = GetLabelOrName(active);

        // Asegura instancia y guarda en PlayerData
        if (PlayerDataStore.Instance == null) return;                                  // :contentReference[oaicite:2]{index=2}
        if (PlayerDataStore.Instance.Current == null) PlayerDataStore.Instance.Current = new PlayerData(); // :contentReference[oaicite:3]{index=3}
        PlayerDataStore.Instance.Current.emotionalState = value;                        // :contentReference[oaicite:4]{index=4}
    }

    private string GetLabelOrName(Toggle t)
    {
        var label = t.GetComponentInChildren<TMP_Text>();
        return (label && !string.IsNullOrWhiteSpace(label.text)) ? label.text.Trim()
                                                                 : t.gameObject.name.Trim();
    }
}
