using UnityEngine;

public class Crowd : MonoBehaviour
{
    [Range(0, 1)] public float defaultSpeed;
    [Range(1, 5)] public float cheeringSpeed;
    [Range(0, 1)] public float randomnessFactor;
    public float maximumHeight;

    [HideInInspector] public float currentSpeedFactor;

    private void Awake()
    {
        currentSpeedFactor = defaultSpeed;
    }

    private void UpdateState(string state)
    {
        switch (state)
        {
            case "Idle":
                currentSpeedFactor = defaultSpeed;
                //Set the speed to default value
                break;

            case "Cheer":
                currentSpeedFactor = cheeringSpeed;
                //Set the speed to cheering value
                //Here you can play anything, maybe a cheering sound, fireworks, an
                break;
        }
    }
}
