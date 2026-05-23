using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

/// <summary>
/// Utilidades relacionadas con el manejo de los puntos del juego.
/// </summary>
public static class ScoreUtils
{
    /// <summary>
    /// Redondea la puntuación a un valor entero.
    /// </summary>
    /// <param name="score">Puntos a redondear.</param>
    /// <returns>String de los puntos redondeado a INT.</returns>
    public static string RoundScoreToInt(float score)
    {
        return Mathf.RoundToInt(score).ToString();
    }
}
