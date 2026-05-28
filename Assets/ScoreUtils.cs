using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public static class ScoreUtils
{
    // Redondea el score y lo devuelve como texto
    public static string RoundScoreToInt(float score)
    {
        return Mathf.RoundToInt(score).ToString();
    }
}