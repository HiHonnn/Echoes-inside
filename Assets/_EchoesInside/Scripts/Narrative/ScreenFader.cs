using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public sealed class ScreenFader : MonoBehaviour
{
    [SerializeField, Min(0f)] private float fadeDuration = 0.75f;
    [SerializeField] private bool fadeInOnStart = true;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private CanvasGroup canvasGroup;
    private Coroutine activeFade;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = canvasGroup.alpha > 0f;
    }

    private void Start()
    {
        if (fadeInOnStart)
        {
            canvasGroup.alpha = 1f;
            FadeIn();
        }
    }

    [ContextMenu("Fade In")]
    public void FadeIn()
    {
        FadeIn(null);
    }

    public void FadeIn(Action onComplete)
    {
        BeginFade(0f, onComplete);
    }

    [ContextMenu("Fade Out")]
    public void FadeOut()
    {
        FadeOut(null);
    }

    public void FadeOut(Action onComplete)
    {
        BeginFade(1f, onComplete);
    }

    public void FadeOutAndLoadScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("ScreenFader cannot load a scene because the scene name is empty.", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError(
                $"ScreenFader cannot load scene '{sceneName}'. Add it to the active Build Profile first.",
                this);
            return;
        }

        SceneMusicPlayer sceneMusicPlayer = FindFirstObjectByType<SceneMusicPlayer>();
        if (sceneMusicPlayer != null)
        {
            sceneMusicPlayer.FadeOut(fadeDuration);
        }

        BeginFade(1f, () => SceneManager.LoadScene(sceneName));
    }

    private void BeginFade(float targetAlpha, Action onComplete = null)
    {
        if (activeFade != null)
        {
            StopCoroutine(activeFade);
            activeFade = null;
        }

        canvasGroup.blocksRaycasts = true;

        if (fadeDuration <= 0f)
        {
            CompleteFade(targetAlpha, onComplete);
            return;
        }

        activeFade = StartCoroutine(FadeRoutine(targetAlpha, onComplete));
    }

    private IEnumerator FadeRoutine(float targetAlpha, Action onComplete)
    {
        float startingAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float linearProgress = Mathf.Clamp01(elapsed / fadeDuration);
            float easedProgress = fadeCurve.Evaluate(linearProgress);
            canvasGroup.alpha = Mathf.Lerp(startingAlpha, targetAlpha, easedProgress);
            yield return null;
        }

        activeFade = null;
        CompleteFade(targetAlpha, onComplete);
    }

    private void CompleteFade(float targetAlpha, Action onComplete)
    {
        canvasGroup.alpha = targetAlpha;
        canvasGroup.blocksRaycasts = targetAlpha > 0f;
        onComplete?.Invoke();
    }

    private void OnValidate()
    {
        fadeDuration = Mathf.Max(0f, fadeDuration);

        if (fadeCurve == null || fadeCurve.length == 0)
        {
            fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }
    }
}
