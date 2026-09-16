using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class StaticCutscenePlayer : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform dialoguePanel;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text contentText;

    [Header("Cutscene")]
    [SerializeField] private List<CutsceneLine> lines = new List<CutsceneLine>();
    [SerializeField] private bool playOnStart = true;

    [Header("Optional Background")]
    [SerializeField] private Image backgroundImage;
    [SerializeField, Min(0f)] private float backgroundFadeDuration = 0.6f;

    [Header("Optional Audio")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioSource soundEffectSource;

    [Header("Events")]
    [SerializeField] private UnityEvent onCutsceneFinished = new UnityEvent();

    private int currentLineIndex = -1;
    private bool isPlaying;
    private bool hasDefaultPanelPosition;
    private Vector3 defaultPanelPosition;
    private bool hasDefaultSpeakerColor;
    private Color defaultSpeakerColor;
    private Sprite defaultBackgroundSprite;
    private Color defaultBackgroundColor;
    private Coroutine backgroundFade;
    private bool isBackgroundTransitioning;

    public bool IsPlaying => isPlaying;
    public int CurrentLineIndex => currentLineIndex;
    public UnityEvent OnCutsceneFinished => onCutsceneFinished;

    private void Awake()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.gameObject.SetActive(false);
        }

        if (backgroundImage != null)
        {
            defaultBackgroundSprite = backgroundImage.sprite;
            defaultBackgroundColor = backgroundImage.color;
        }
    }

    private void Start()
    {
        if (playOnStart)
        {
            Play();
        }
    }

    private void Update()
    {
        if (!isPlaying)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.Return) ||
            Input.GetKeyDown(KeyCode.E))
        {
            Advance();
        }
    }

    [ContextMenu("Play Cutscene")]
    public void Play()
    {
        if (!HasRequiredReferences())
        {
            return;
        }

        if (!hasDefaultPanelPosition)
        {
            defaultPanelPosition = dialoguePanel.position;
            hasDefaultPanelPosition = true;
        }

        if (!hasDefaultSpeakerColor)
        {
            defaultSpeakerColor = speakerText.color;
            hasDefaultSpeakerColor = true;
        }

        ResetBackground();
        currentLineIndex = -1;
        isPlaying = true;
        dialoguePanel.gameObject.SetActive(true);
        PlayBackgroundMusic();

        if (lines == null || lines.Count == 0)
        {
            Debug.LogWarning("StaticCutscenePlayer has no dialogue lines to play.", this);
            Finish();
            return;
        }

        Advance();
    }

    [ContextMenu("Advance Cutscene")]
    public void Advance()
    {
        if (!isPlaying || isBackgroundTransitioning)
        {
            return;
        }

        currentLineIndex++;

        if (currentLineIndex >= lines.Count)
        {
            Finish();
            return;
        }

        DisplayLine(lines[currentLineIndex]);
    }

    private void DisplayLine(CutsceneLine line)
    {
        if (line == null)
        {
            Debug.LogWarning($"StaticCutscenePlayer line {currentLineIndex} is empty.", this);
            Advance();
            return;
        }

        bool hasSpeaker = !string.IsNullOrWhiteSpace(line.SpeakerName);
        speakerText.gameObject.SetActive(hasSpeaker);
        speakerText.text = hasSpeaker ? line.SpeakerName : string.Empty;
        speakerText.color = line.SpeakerColor.a > 0f
            ? line.SpeakerColor
            : defaultSpeakerColor;
        contentText.text = line.Content ?? string.Empty;

        dialoguePanel.position = line.Anchor != null
            ? line.Anchor.position
            : defaultPanelPosition;

        if (line.SoundEffect != null && soundEffectSource != null)
        {
            soundEffectSource.PlayOneShot(line.SoundEffect);
        }

        TryChangeBackground(line.BackgroundSprite);
    }

    private void TryChangeBackground(Sprite nextSprite)
    {
        if (nextSprite == null)
        {
            return;
        }

        if (backgroundImage == null)
        {
            Debug.LogWarning(
                $"StaticCutscenePlayer line {currentLineIndex} has a background sprite, " +
                "but Background Image is not assigned.",
                this);
            return;
        }

        if (backgroundImage.sprite == nextSprite)
        {
            return;
        }

        StopBackgroundFade();

        if (backgroundFadeDuration <= 0f)
        {
            backgroundImage.sprite = nextSprite;
            backgroundImage.color = defaultBackgroundColor;
            return;
        }

        backgroundFade = StartCoroutine(FadeBackground(nextSprite));
    }

    private IEnumerator FadeBackground(Sprite nextSprite)
    {
        isBackgroundTransitioning = true;
        float halfDuration = backgroundFadeDuration * 0.5f;
        Color blackColor = new Color(0f, 0f, 0f, defaultBackgroundColor.a);

        yield return FadeBackgroundColor(backgroundImage.color, blackColor, halfDuration);
        backgroundImage.sprite = nextSprite;
        yield return FadeBackgroundColor(blackColor, defaultBackgroundColor, halfDuration);

        backgroundImage.color = defaultBackgroundColor;
        isBackgroundTransitioning = false;
        backgroundFade = null;
    }

    private IEnumerator FadeBackgroundColor(Color from, Color to, float duration)
    {
        if (duration <= 0f)
        {
            backgroundImage.color = to;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            backgroundImage.color = Color.Lerp(from, to, easedProgress);
            yield return null;
        }

        backgroundImage.color = to;
    }

    private void ResetBackground()
    {
        StopBackgroundFade();

        if (backgroundImage == null)
        {
            return;
        }

        backgroundImage.sprite = defaultBackgroundSprite;
        backgroundImage.color = defaultBackgroundColor;
    }

    private void StopBackgroundFade()
    {
        if (backgroundFade != null)
        {
            StopCoroutine(backgroundFade);
            backgroundFade = null;
        }

        isBackgroundTransitioning = false;
    }

    private void PlayBackgroundMusic()
    {
        if (musicSource == null)
        {
            return;
        }

        AudioClip clipToPlay = backgroundMusic != null ? backgroundMusic : musicSource.clip;
        if (clipToPlay == null)
        {
            return;
        }

        musicSource.clip = clipToPlay;
        musicSource.loop = true;

        if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    private bool HasRequiredReferences()
    {
        if (dialoguePanel != null && speakerText != null && contentText != null)
        {
            return true;
        }

        Debug.LogError(
            "StaticCutscenePlayer requires Dialogue Panel, Speaker Text, and Content Text references.",
            this);
        return false;
    }

    private void Finish()
    {
        isPlaying = false;
        dialoguePanel.gameObject.SetActive(false);
        onCutsceneFinished.Invoke();
    }

    private void OnDisable()
    {
        StopBackgroundFade();

        if (backgroundImage != null)
        {
            backgroundImage.color = defaultBackgroundColor;
        }
    }

    private void OnValidate()
    {
        backgroundFadeDuration = Mathf.Max(0f, backgroundFadeDuration);
    }
}
