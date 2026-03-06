using UnityEngine;

public static class PauseController
{
    private static bool isGamePaused = false;

    public static bool IsGamePaused
    {
        get { return isGamePaused; }
    }

    public static void SetPause(bool pause)
    {
        isGamePaused = pause;
        Time.timeScale = pause ? 0f : 1f;
    }
}

