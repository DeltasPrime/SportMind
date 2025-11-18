using UnityEngine;

public class RandomColorOnStart : MonoBehaviour
{
    private void Start()
    {
        ChooseRandomColor();
    }

    private void ChooseRandomColor()
    {
        Renderer renderer = GetComponent<Renderer>();
        Material newMaterial = renderer.material;

        newMaterial.color = new Color(
            Random.Range(0, 256) / 255f,
            Random.Range(0, 256) / 255f,
            Random.Range(0, 256) / 255f
        );

        renderer.material = newMaterial;
    }
}
