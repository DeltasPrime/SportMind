// Editor/FindMissingScripts.cs
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public class FindMissingScript : EditorWindow
{
    [MenuItem("Tools/Find Missing Scripts in Scene")]
    public static void Find()
    {
        int count = 0;
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            // Ignora assets fuera de la escena
            if (EditorUtility.IsPersistent(go)) continue;

            var comps = go.GetComponents<Component>();
            for (int i = 0; i < comps.Length; i++)
            {
                if (comps[i] == null)
                {
                    Debug.LogWarning($"Missing Script en: {GetPath(go)}", go);
                    count++;
                }
            }
        }
        Debug.Log($"Revisión completa. Missing scripts: {count}");
    }

    static string GetPath(GameObject obj)
    {
        string path = obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = obj.name + "/" + path;
        }
        return path;
    }
}
#endif
