using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class TitleScreenController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button startButton;
    [SerializeField] private ScreenFader screenFader;

    [Header("Transition")]
    [SerializeField] private string introSceneName = "Intro_DinnerScene";

    private bool transitionStarted;

    private void Reset()
    {
        startButton = GetComponent<Button>();
        screenFader = FindFirstObjectByType<ScreenFader>();
    }

    private void OnEnable()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
        }
    }

    private void Start()
    {
        if (startButton != null && EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(startButton.gameObject);
        }
    }

    private void OnDisable()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(StartGame);
        }
    }

    public void StartGame()
    {
        if (transitionStarted)
        {
            return;
        }

        if (startButton == null)
        {
            Debug.LogError(
                "TitleScreenController cannot start because Start Button is not assigned.",
                this);
            return;
        }

        if (string.IsNullOrWhiteSpace(introSceneName))
        {
            Debug.LogError(
                "TitleScreenController cannot start because Intro Scene Name is empty.",
                this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(introSceneName))
        {
            Debug.LogError(
                $"TitleScreenController cannot load scene '{introSceneName}'. " +
                "Add it to the active Build Profile first.",
                this);
            return;
        }

        if (screenFader == null)
        {
            Debug.LogError(
                "TitleScreenController cannot start because Screen Fader is not assigned.",
                this);
            return;
        }

        transitionStarted = true;
        startButton.interactable = false;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        screenFader.FadeOutAndLoadScene(introSceneName);
    }
}
