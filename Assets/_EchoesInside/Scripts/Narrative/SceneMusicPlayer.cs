using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public sealed class SceneMusicPlayer : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private AudioClip musicClip;
    [SerializeField, Range(0f, 1f)] private float targetVolume = 0.3f;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool playOnStart = true;

    [Header("Fade")]
    [SerializeField, Min(0f)] private float fadeInDuration = 1.2f;

    private AudioSource audioSource;
    private Coroutine volumeFade;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void Start()
    {
        if (playOnStart)
        {
            Play();
        }
    }

    public AudioClip CurrentClip => audioSource != null && audioSource.clip != null
        ? audioSource.clip
        : musicClip;

    public void SetClip(AudioClip clip)
    {
        musicClip = clip;

        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.clip = clip;
        }
    }

    public void TransitionTo(AudioClip nextClip, float fadeDuration)
    {
        if (nextClip == null)
        {
            Debug.LogWarning("SceneMusicPlayer cannot transition to an empty clip.", this);
            return;
        }

        if (audioSource.isPlaying && audioSource.clip == nextClip)
        {
            musicClip = nextClip;
            return;
        }

        StopVolumeFade();
        musicClip = nextClip;
        fadeDuration = Mathf.Max(0f, fadeDuration);

        if (fadeDuration <= 0f)
        {
            StartClipImmediately(nextClip);
            return;
        }

        volumeFade = StartCoroutine(TransitionClip(nextClip, fadeDuration));
    }

    [ContextMenu("Play Music")]
    public void Play()
    {
        AudioClip clipToPlay = musicClip != null ? musicClip : audioSource.clip;
        if (clipToPlay == null)
        {
            Debug.LogWarning("SceneMusicPlayer has no music clip assigned.", this);
            return;
        }

        if (audioSource.isPlaying && audioSource.clip == clipToPlay)
        {
            return;
        }

        StopVolumeFade();
        audioSource.clip = clipToPlay;
        audioSource.loop = loop;

        if (fadeInDuration <= 0f)
        {
            audioSource.volume = targetVolume;
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
            return;
        }

        audioSource.volume = 0f;
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        volumeFade = StartCoroutine(FadeVolume(0f, targetVolume, fadeInDuration, false));
    }

    public void FadeOut(float duration)
    {
        StopVolumeFade();

        if (duration <= 0f)
        {
            audioSource.volume = 0f;
            audioSource.Stop();
            return;
        }

        volumeFade = StartCoroutine(
            FadeVolume(audioSource.volume, 0f, duration, true));
    }

    private IEnumerator FadeVolume(
        float startingVolume,
        float target,
        float duration,
        bool stopOnComplete)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            audioSource.volume = Mathf.Lerp(startingVolume, target, easedProgress);
            yield return null;
        }

        audioSource.volume = target;
        if (stopOnComplete)
        {
            audioSource.Stop();
        }

        volumeFade = null;
    }

    private void StartClipImmediately(AudioClip clip)
    {
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.loop = loop;
        audioSource.volume = targetVolume;
        audioSource.Play();
    }

    private IEnumerator TransitionClip(AudioClip nextClip, float fadeDuration)
    {
        if (audioSource.isPlaying)
        {
            yield return LerpVolume(audioSource.volume, 0f, fadeDuration);
            audioSource.Stop();
        }

        audioSource.clip = nextClip;
        audioSource.loop = loop;
        audioSource.volume = 0f;
        audioSource.Play();

        yield return LerpVolume(0f, targetVolume, fadeDuration);
        audioSource.volume = targetVolume;
        volumeFade = null;
    }

    private IEnumerator LerpVolume(float from, float to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            audioSource.volume = Mathf.Lerp(from, to, easedProgress);
            yield return null;
        }

        audioSource.volume = to;
    }

    private void StopVolumeFade()
    {
        if (volumeFade == null)
        {
            return;
        }

        StopCoroutine(volumeFade);
        volumeFade = null;
    }

    private void OnDisable()
    {
        StopVolumeFade();
    }

    private void OnValidate()
    {
        targetVolume = Mathf.Clamp01(targetVolume);
        fadeInDuration = Mathf.Max(0f, fadeInDuration);
    }
}
