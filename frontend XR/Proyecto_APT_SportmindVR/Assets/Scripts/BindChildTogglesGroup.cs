using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(ToggleGroup))]
public class BindChildTogglesToGroup : MonoBehaviour
{
    [Tooltip("Solo informa problemas, no re-asigna (desactívalo si quieres que fuerce la asignación).")]
    public bool dryRun = false;

    void Awake()
    {
        var group = GetComponent<ToggleGroup>();
        var toggles = GetComponentsInChildren<Toggle>(true);

        foreach (var t in toggles)
        {
            if (t == null) continue;

            // Si el toggle ya apunta a OTRA instancia de ToggleGroup, lo marcamos.
            if (t.group != null && t.group != group)
            {
                Debug.LogWarning(
                    $"[BindAndVerifyChildToggles] {t.name} está enlazado a OTRO ToggleGroup ({t.group.name}). " +
                    $"Debería pertenecer al grupo de {name}.",
                    t
                );
            }

            if (!dryRun)
                t.group = group; // fuerza asignación correcta
        }
    }
}
