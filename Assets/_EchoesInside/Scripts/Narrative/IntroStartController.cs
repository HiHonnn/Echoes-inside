using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class IntroStartController : MonoBehaviour
{
    [Header("Intro References")]
    [SerializeField] private StaticCutscenePlayer cutscenePlayer;
    [SerializeField] private ScreenFader screenFader;
    [SerializeField] private AudioSource musicSource;

    [Header("Transition")]
    [SerializeField] private string mainMapSceneName = "SampleScene";
    [SerializeField, Min(0f)] private float musicFadeDuration = 1.2f;

    private bool transitionStarted;

    private void Reset()
    {
        cutscenePlayer = GetComponent<StaticCutscenePlayer>();
    }

    private void OnEnable()
    {
        if (cutscenePlayer != null)
        {
            cutscenePlayer.OnCutsceneFinished.AddListener(BeginTransition);
        }
    }

    private void OnDisable()
    {
        if (cutscenePlayer != null)
        {
            cutscenePlayer.OnCutsceneFinished.RemoveListener(BeginTransition);
        }
    }

    [ContextMenu("Begin Transition")]
    public void BeginTransition()
    {
        if (transitionStarted)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(mainMapSceneName))
        {
            Debug.LogError(
                "IntroStartController cannot transition because Main Map Scene Name is empty.",
                this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(mainMapSceneName))
        {
            Debug.LogError(
                $"IntroStartController cannot load scene '{mainMapSceneName}'. Add it to the active Build Profile first.",
                this);
            return;
        }

        transitionStarted = true;

        if (screenFader == null)
        {
            Debug.LogWarning(
                "IntroStartController has no ScreenFader. Loading the Main Map without a fade.",
                this);
            StopMusicImmediately();
            SceneManager.LoadScene(mainMapSceneName);
            return;
        }

        if (musicSource != null && musicSource.isPlaying)
        {
            StartCoroutine(FadeOutMusic());
        }

        screenFader.FadeOutAndLoadScene(mainMapSceneName);
    }

    private IEnumerator FadeOutMusic()
    {
        float startingVolume = musicSource.volume;

        if (musicFadeDuration <= 0f)
        {
            StopMusicImmediately();
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < musicFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / musicFadeDuration);
            musicSource.volume = Mathf.Lerp(startingVolume, 0f, Mathf.SmoothStep(0f, 1f, progress));
            yield return null;
        }

        StopMusicImmediately();
    }

    private void StopMusicImmediately()
    {
        if (musicSource == null)
        {
            return;
        }

        musicSource.volume = 0f;
        musicSource.Stop();
    }

    private void OnValidate()
    {
        musicFadeDuration = Mathf.Max(0f, musicFadeDuration);
    }
}
