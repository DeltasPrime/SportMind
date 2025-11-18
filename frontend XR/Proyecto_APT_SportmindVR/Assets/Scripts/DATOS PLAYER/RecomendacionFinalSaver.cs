using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecomendacionFinalSaver : MonoBehaviour
{
    [SerializeField] private ToggleGroup group;
    [SerializeField] private Toggle[] toggles;

    public void SaveRecomendation()
    {
        if (group == null || toggles == null) return;

        foreach (var t in toggles)
        {
            if (t != null && t.isOn)
            {
                TMP_Text txt = t.GetComponentInChildren<TMP_Text>();
                if (txt == null) return;

                string textValue = txt.text.Trim();

                                int intValue;
                if (!int.TryParse(textValue, out intValue))
                {
                    Debug.LogWarning("El texto del toggle no es un número válido: " + textValue);
                    return;
                }

                if (PlayerDataStore.Instance == null) return;
                if (PlayerDataStore.Instance.Current == null)
                    PlayerDataStore.Instance.Current = new PlayerData();

                PlayerDataStore.Instance.Current.recomendacionFinal = intValue;
                return;
            }
        }
    }
}
