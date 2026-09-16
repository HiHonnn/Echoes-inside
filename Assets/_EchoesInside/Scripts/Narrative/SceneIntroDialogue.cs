using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public sealed class SceneIntroDialogue : MonoBehaviour
{
    private static bool hasPlayedThisSession;

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text contentText;

    [Header("Player Control")]
    [SerializeField] private MonoBehaviour playerController;

    [Header("Dialogue")]
    [SerializeField] private string speakerName = "Mình";
    [SerializeField] private List<string> lines = new List<string>
    {
        "Mình... đang ở đâu?",
        "Đây là nhà mình mà. Nhưng sao mọi thứ tối như vậy?",
        "Những cánh cửa kia... có gì đó không giống bình thường.",
        "Nếu ánh sáng đã biến mất từ bên trong, có lẽ mình phải bắt đầu từ từng căn phòng."
    };
    [SerializeField, Min(0f)] private float initialDelay = 1.2f;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool playOncePerSession = true;

    [Header("Events")]
    [SerializeField] private UnityEvent onDialogueFinished = new UnityEvent();

    private int currentLineIndex = -1;
    private bool isPlaying;
    private bool isReadyForInput;
    private bool playerControllerWasEnabled;
    private Coroutine delayedStart;

    public bool IsPlaying => isPlaying;
    public UnityEvent OnDialogueFinished => onDialogueFinished;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetSessionState()
    {
        hasPlayedThisSession = false;
    }

    private void Awake()
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    private void Start()
    {
        if (playOnStart)
        {
            StartDialogue();
        }
    }

    private void Update()
    {
        if (!isPlaying || !isReadyForInput)
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

    [ContextMenu("Start Dialogue")]
    public void StartDialogue()
    {
        if (isPlaying)
        {
            return;
        }

        if (playOncePerSession && hasPlayedThisSession)
        {
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(false);
            }

            return;
        }

        if (!HasRequiredReferences())
        {
            return;
        }

        if (lines == null || lines.Count == 0)
        {
            Debug.LogError("SceneIntroDialogue requires at least one dialogue line.", this);
            return;
        }

        if (playOncePerSession)
        {
            hasPlayedThisSession = true;
        }

        currentLineIndex = 0;
        isPlaying = true;
        isReadyForInput = false;
        LockPlayerControl();

        if (initialDelay <= 0f)
        {
            ShowCurrentLine();
            return;
        }

        delayedStart = StartCoroutine(ShowAfterDelay());
    }

    [ContextMenu("Advance Dialogue")]
    public void Advance()
    {
        if (!isPlaying || !isReadyForInput)
        {
            return;
        }

        currentLineIndex++;

        if (currentLineIndex >= lines.Count)
        {
            FinishDialogue();
            return;
        }

        ShowCurrentLine();
    }

    private IEnumerator ShowAfterDelay()
    {
        yield return new WaitForSecondsRealtime(initialDelay);
        delayedStart = null;

        if (isPlaying)
        {
            ShowCurrentLine();
        }
    }

    private void ShowCurrentLine()
    {
        dialoguePanel.SetActive(true);

        if (speakerText != null)
        {
            speakerText.gameObject.SetActive(!string.IsNullOrWhiteSpace(speakerName));
            speakerText.text = speakerName;
        }

        contentText.text = lines[currentLineIndex] ?? string.Empty;
        isReadyForInput = true;
    }

    private void LockPlayerControl()
    {
        if (playerController == null)
        {
            return;
        }

        playerControllerWasEnabled = playerController.enabled;
        playerController.enabled = false;

        Rigidbody2D rigidbody = playerController.GetComponent<Rigidbody2D>();
        if (rigidbody != null)
        {
            rigidbody.linearVelocity = Vector2.zero;
        }
    }

    private void RestorePlayerControl()
    {
        if (playerController != null)
        {
            playerController.enabled = playerControllerWasEnabled;
        }
    }

    private void FinishDialogue()
    {
        isPlaying = false;
        isReadyForInput = false;
        dialoguePanel.SetActive(false);
        RestorePlayerControl();
        onDialogueFinished.Invoke();
    }

    private bool HasRequiredReferences()
    {
        if (dialoguePanel != null && contentText != null)
        {
            return true;
        }

        Debug.LogError(
            "SceneIntroDialogue requires Dialogue Panel and Content Text references.",
            this);
        return false;
    }

    private void OnDisable()
    {
        if (delayedStart != null)
        {
            StopCoroutine(delayedStart);
            delayedStart = null;
        }

        if (!isPlaying)
        {
            return;
        }

        isPlaying = false;
        isReadyForInput = false;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        RestorePlayerControl();
    }

    private void OnValidate()
    {
        initialDelay = Mathf.Max(0f, initialDelay);
    }
}
