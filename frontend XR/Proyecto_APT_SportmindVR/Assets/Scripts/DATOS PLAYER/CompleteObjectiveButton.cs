using UnityEngine;

public class CompleteObjectiveButton : MonoBehaviour
{
    [Tooltip("Debe ser EXACTO al que esta en el ProgressManager")]
    public string objectiveName;

    public void Complete()
    {
        if (string.IsNullOrWhiteSpace(objectiveName))
        {
            Debug.LogError("No escribiste el nombre del objetivo");
            return;
        }

        if (ProgressManager.Instance == null)
        {
            Debug.LogError("No existe ProgressManager en escena");
            return;
        }

        ProgressManager.Instance.CompleteObjective(objectiveName);
    }
}
