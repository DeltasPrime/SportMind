using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PreEmotionWallEasySaver : MonoBehaviour
{
    [SerializeField] private ToggleGroup group;
    [SerializeField] private Toggle[] toggles; // los 5 toggles (NO valores manuales)

    public void SavePreEmotion()
    {
        if (group == null || toggles == null) return;

        foreach (var t in toggles)
        {
            if (t != null && t.isOn)
            {
                // obtenemos el texto del toggle (primer TMP_Text hijo)
                TMP_Text txt = t.GetComponentInChildren<TMP_Text>();
                if (txt == null) return;

                string value = txt.text.Trim();

                if (PlayerDataStore.Instance == null) return;
                if (PlayerDataStore.Instance.Current == null)
                    PlayerDataStore.Instance.Current = new PlayerData();

                PlayerDataStore.Instance.Current.preEmotionMuroEasy = value;
                return;
            }
        }
    }
}