using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Objective { public string objectiveName; public bool isCompleted; }

public class ProgressManager : MonoBehaviour
{
    public static ProgressManager Instance;

    [Header("Lista de Objetivos (Inspector)")]
    public List<Objective> objectives = new List<Objective>();

    private Dictionary<string, Objective> dict;   // cache

    // ---------- BOOTSTRAP ROBUSTO ----------
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (transform.parent != null) transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        EnsureInitialized();
    }

    // Crea el manager si no existe en ninguna escena (útil si te olvidaste de colocarlo)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoSpawnIfMissing()
    {
        if (Instance == null)
        {
            var go = new GameObject("ProgressManager");
            Instance = go.AddComponent<ProgressManager>();
            Debug.LogWarning("ProgressManager se creó automáticamente. Recuerda agregarlo a tu escena inicial.");
        }
    }

    private void EnsureInitialized()
    {
        if (dict != null) return;

        dict = new Dictionary<string, Objective>();
        foreach (var o in objectives)
        {
            if (o == null || string.IsNullOrWhiteSpace(o.objectiveName)) continue;
            if (!dict.ContainsKey(o.objectiveName)) dict.Add(o.objectiveName, o);
        }
    }

    // ---------- API ----------
    public void CompleteObjective(string objectiveName)
    {
        if (Instance == null)
        {
            Debug.LogError("ProgressManager.Instance es null. ¿Existe en escena?");
            return;
        }

        EnsureInitialized();

        if (string.IsNullOrWhiteSpace(objectiveName))
        {
            Debug.LogError("Nombre de objetivo vacío/nulo.");
            return;
        }

        if (!dict.TryGetValue(objectiveName, out var obj))
        {
            Debug.LogWarning($"Objetivo '{objectiveName}' no existe en la lista del Inspector.");
            return;
        }

        obj.isCompleted = true;
        Debug.Log($"Objetivo '{objectiveName}' completado.");
    }

    public bool IsObjectiveCompleted(string objectiveName)
    {
        EnsureInitialized();
        return dict.TryGetValue(objectiveName, out var obj) && obj.isCompleted;
    }

    public bool CheckAllObjectivesCompleted()
    {
        EnsureInitialized();
        foreach (var o in objectives)
        {
            if (o == null) continue;
            if (!o.isCompleted)
            {
                Debug.Log($"Falta: {o.objectiveName}");
                return false;
            }
        }
        Debug.Log("¡Todos los objetivos completados!");
        return true;
    }

    public void ResetAll()
    {
        EnsureInitialized();
        foreach (var o in objectives) if (o != null) o.isCompleted = false;
    }
}
