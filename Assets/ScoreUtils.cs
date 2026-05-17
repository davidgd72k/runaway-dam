using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public static class ScoreUtils
{
    public static string RoundScoreToInt(float score)
    {
        return Mathf.RoundToInt(score).ToString();
    }
}
