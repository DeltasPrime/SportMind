using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerFormSaveMain : MonoBehaviour
{
    [Header("UI refs")]
    [SerializeField] TMP_InputField nameField;
    [SerializeField] TMP_Dropdown sportDropdown; // Atletismo, Judo, Natación
    [SerializeField] ToggleGroup genderGroup;
    [SerializeField] Toggle maleToggle;   // nómbralos "Masculino" / "Femenino" o que el label tenga ese texto
    [SerializeField] Toggle femaleToggle;

    readonly string[] sportOptions = { "Atletismo", "Judo", "Natación" };

    public void OnNextPressed()
    {
        if (PlayerDataStore.Instance == null)
        {
            Debug.LogError("PlayerDataStore no está en escena.");
            return;
        }
        if (PlayerDataStore.Instance.Current == null)
            PlayerDataStore.Instance.Current = new PlayerData(); // crea si faltara  :contentReference[oaicite:2]{index=2}

        var data = PlayerDataStore.Instance.Current;

        // Nombre
        data.playerName = nameField ? nameField.text : "";      // :contentReference[oaicite:3]{index=3}

        // Deporte
        if (sportDropdown)
        {
            int i = Mathf.Clamp(sportDropdown.value, 0, sportOptions.Length - 1);
            data.selectedSport = sportOptions[i];               // :contentReference[oaicite:4]{index=4}
        }

        // ---- Género: método robusto ----
        string selectedGender = GetSelectedGender();
        if (!string.IsNullOrEmpty(selectedGender))
        {
            data.gender = selectedGender;                       // :contentReference[oaicite:5]{index=5}
        }
        else
        {
            Debug.LogWarning("No hay Toggle de género activo. Mantengo el valor previo: " + (data.gender ?? "<vacío>"));
            // Si quieres forzar un default:
            // data.gender = "Masculino";
        }

    }

    private string GetSelectedGender()
    {
        // 1) Chequeo directo por referencias
        if (maleToggle && maleToggle.isOn) return GetToggleTextOrName(maleToggle);
        if (femaleToggle && femaleToggle.isOn) return GetToggleTextOrName(femaleToggle);

        // 2) Recorro el grupo por si hay más toggles o no asignaste male/female
        if (genderGroup)
        {
            var toggles = genderGroup.GetComponentsInChildren<Toggle>(includeInactive: false);
            foreach (var t in toggles)
            {
                if (t != null && t.isOn)
                    return GetToggleTextOrName(t);
            }
        }

        // 3) Nada seleccionado
        return null;
    }

    private string GetToggleTextOrName(Toggle t)
    {
        // Usa el label (TMP_Text) si existe; si no, el nombre del GO
        var label = t.GetComponentInChildren<TMP_Text>();
        if (label && !string.IsNullOrWhiteSpace(label.text))
            return label.text.Trim();
        return t.gameObject.name.Trim();
    }
}
