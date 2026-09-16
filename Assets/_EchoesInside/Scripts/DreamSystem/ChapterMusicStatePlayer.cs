using UnityEngine;

public enum HealingMusicChapter
{
    Grandma,
    Father,
    Mother
}

[DisallowMultipleComponent]
[RequireComponent(typeof(SceneMusicPlayer))]
public sealed class ChapterMusicStatePlayer : MonoBehaviour
{
    [Header("Chapter")]
    [SerializeField] private HealingMusicChapter chapter;

    [Header("Music")]
    [SerializeField] private SceneMusicPlayer musicPlayer;
    [SerializeField] private AudioClip beforeHealingClip;
    [SerializeField] private AudioClip healedClip;
    [SerializeField, Min(0f)] private float transitionDuration = 1.2f;

    private void Reset()
    {
        musicPlayer = GetComponent<SceneMusicPlayer>();
    }

    private void Awake()
    {
        if (musicPlayer == null)
        {
            musicPlayer = GetComponent<SceneMusicPlayer>();
        }

        AudioClip initialClip = ShouldUseHealedMusic()
            ? healedClip
            : beforeHealingClip;

        if (initialClip == null)
        {
            Debug.LogWarning(
                "ChapterMusicStatePlayer has no clip for the current state.",
                this);
            return;
        }

        musicPlayer.SetClip(initialClip);
    }

    public void PlayHealedMusic()
    {
        if (musicPlayer == null || healedClip == null)
        {
            Debug.LogWarning(
                "ChapterMusicStatePlayer cannot play healed music.",
                this);
            return;
        }

        musicPlayer.TransitionTo(healedClip, transitionDuration);
    }

    private bool ShouldUseHealedMusic()
    {
        if (GameManager.Instance == null)
        {
            return false;
        }

        switch (chapter)
        {
            case HealingMusicChapter.Grandma:
                Chapter1State grandma = GameManager.Instance.chapter1State;
                return grandma.framePickedUp || grandma.dialogueCompleted;

            case HealingMusicChapter.Father:
                Chapter2State father = GameManager.Instance.chapter2State;
                return father.mazeCompleted || father.dialogueCompleted;

            case HealingMusicChapter.Mother:
                Chapter3State mother = GameManager.Instance.chapter3State;
                return mother.mealCompleted || mother.dialogueCompleted;

            default:
                return false;
        }
    }

    private void OnValidate()
    {
        transitionDuration = Mathf.Max(0f, transitionDuration);
    }
}
