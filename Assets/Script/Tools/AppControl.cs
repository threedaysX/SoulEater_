using UnityEngine;

public class AppControl : Singleton<AppControl>
{
    [SerializeField] private bool on;
    public GameObject gamePauseBlocker;
    public static bool GamePaused { get; private set; }
    public static bool MenuPaused { get; private set; }
    public static bool RunInBackground { get; private set; }

    private void Start()
    {
        on = true;
#if UNITY_EDITOR
        on = false;
#endif
    }

    public void SetAppRunInBackground(bool runInBackground)
    {
        Application.runInBackground = runInBackground;
        RunInBackground = runInBackground;
    }

    public static bool IsGamePaused()
    {
        return (!RunInBackground && GamePaused) || MenuPaused;
    }

    public static void MenuPausedGame(bool paused)
    {
        MenuPaused = paused;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        GamePaused = !hasFocus;
        if (on)
        {
            SetPauseTimeScale(GamePaused);
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        GamePaused = pauseStatus;
        if (on)
        {
            SetPauseTimeScale(GamePaused);
        }
    }

    private void SetPauseTimeScale(bool pause)
    {
        TimeScaleController.Instance.FocusGame(!pause);
        gamePauseBlocker.SetActive(pause);
    }
}
