using UnityEngine;

public class TestPlayerFiller : MonoBehaviour
{
    [ContextMenu("Rellenar datos de testeo")]
    public void FillTestData()
    {
        if (PlayerDataStore.Instance == null)
        {
            Debug.LogWarning("PlayerDataStore.Instance es null. Asegúrate de que exista en la escena inicial.");
            return;
        }

        if (PlayerDataStore.Instance.Current == null)
        {
            PlayerDataStore.Instance.Current = new PlayerData();
        }

        var data = PlayerDataStore.Instance.Current;

        // Datos de ejemplo
        data.playerName = "Testeo";
        data.selectedSport = "Judo";
        data.gender = "Masculino";
        data.emotionalState = "Enojado";

        // Asumiendo que estas emociones pre son string en tu PlayerData:
        data.preEmotionTiroEasy = "3";
        data.preEmotionTiroHard = "4";
        data.preEmotionMuroEasy = "3";
        data.preEmotionMuroHard = "4";

        // Scores (floats probablemente)
        data.shootingScoreEasy = 30.0f;
        data.shootingScoreHard = 15.0f;

        // Resultados post Tiro
        data.shootingPostEmotion = "Nervioso";
        data.shootingRendimiento = 4;
        data.shootingRitmo = 4;
        data.shootingConfianza = 4;

        // Climbing / Muro
        data.climbingTimeEasy = 4.820000171661377f;
        data.climbingTimeHard = 0.8500000238418579f;
        data.climbingPostEmotion = "Nervioso";
        data.climbingRendimiento = 4;
        data.climbingRitmo = 3;
        data.climbingConfianza = 1;

        // Recomendación final
        data.recomendacionFinal = 5;

        Debug.Log("Datos de testeo cargados en PlayerDataStore.Current");
    }
}
