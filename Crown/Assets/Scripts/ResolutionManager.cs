using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public static class ResolutionManager
{
    private const int W = 1920;
    private const int H = 1080;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        Screen.SetResolution(W, H, FullScreenMode.FullScreenWindow);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (var scaler in Object.FindObjectsOfType<CanvasScaler>())
            Apply(scaler);
    }

    private static void Apply(CanvasScaler scaler)
    {
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(W, H);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }
}
