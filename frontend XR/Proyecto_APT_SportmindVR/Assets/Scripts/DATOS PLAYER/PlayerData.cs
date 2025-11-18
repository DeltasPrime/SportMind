using System;
using UnityEngine;

[Serializable]
public class PlayerData
{
    public string playerName;
    public string selectedSport;
    public string gender;
    public string emotionalState;

    public string preEmotionTiroEasy;
    public string preEmotionTiroHard;
    public string preEmotionMuroEasy;
    public string preEmotionMuroHard;

    public float shootingScoreEasy;
    public float shootingScoreHard;
    public string shootingPostEmotion;

    public int shootingRendimiento;
    public int shootingRitmo;
    public int shootingConfianza;

    public float climbingTimeEasy;
    public float climbingTimeHard;
    public string climbingPostEmotion;

    public int climbingRendimiento;
    public int climbingRitmo;
    public int climbingConfianza;

    public int recomendacionFinal;
}