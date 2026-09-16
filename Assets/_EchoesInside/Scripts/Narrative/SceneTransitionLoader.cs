using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneTransitionLoader
{
    public static void LoadScene(string sceneName, Object context = null)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("SceneTransitionLoader cannot load an empty scene name.", context);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                $"SceneTransitionLoader cannot load scene '{sceneName}'. Add it to the active Build Profile first.",
                context);
            return;
        }

        ScreenFader screenFader = Object.FindFirstObjectByType<ScreenFader>();
        if (screenFader != null)
        {
            screenFader.FadeOutAndLoadScene(sceneName);
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}
