using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gắn lên GameObject chứa script (không cần Collider).
/// Khi người dùng click vào Button "BtnFather" → gọi StartDialogue().
/// Kết thúc hội thoại → đánh dấu Chapter 2 hoàn thành, hiện nút quay lại.
/// </summary>
public class FatherDialogueTrigger : MonoBehaviour
{
    [Header("Thông báo 'Hãy nói chuyện với ba'")]
    [Tooltip("Panel/Text thông báo — ẩn khi bắt đầu hội thoại")]
    [SerializeField] private GameObject talkToFatherNotice;

    [Header("Button click vào ba (ẩn sau khi đã nói)")]
    [Tooltip("Button đè lên hình ba trong Panel_AfterMaze")]
    [SerializeField] private GameObject fatherButton;

    // ── 2 Panel nhân vật ─────────────────────────────────
    [Header("Panel nhân vật BA (hiện khi ba nói)")]
    [SerializeField] private GameObject fatherPanel;
    [SerializeField] private TMP_Text   fatherDialogueText;

    [Header("Panel nhân vật NGƯỜI CHƠI (hiện khi player nói)")]
    [SerializeField] private GameObject playerPanel;
    [SerializeField] private TMP_Text   playerDialogueText;

    // ── Nút điều khiển ───────────────────────────────────
    [Header("Nút điều khiển hội thoại")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button skipButton;

    // ── Nội dung đối thoại ────────────────────────────────
    [Header("Nội dung đối thoại")]
    [SerializeField] private List<FatherDialogueLine> dialogueLines;

    // ── Sau kết thúc ─────────────────────────────────────
    [Header("Sau khi hoàn thành")]
    [Tooltip("FatherRoomManager để gọi OnChapterCompleted()")]
    [SerializeField] private FatherRoomManager roomManager;

    [Tooltip("Panel 'Chapter 2 Hoàn Thành' — hiện sau hội thoại (tùy chọn)")]
    [SerializeField] private GameObject completionPanel;

    // ── Trạng thái nội bộ ────────────────────────────────
    private bool _dialogueStarted = false;
    private int  _dialogueIndex   = 0;

    // ─────────────────────────────────────────────────────
    private void Awake()
    {
        if (dialogueLines == null || dialogueLines.Count == 0)
            dialogueLines = GetDefaultDialogue();
    }

    private void Start()
    {
        // Đăng ký sự kiện nút điều khiển hội thoại
        if (nextButton != null) nextButton.onClick.AddListener(ShowNextLine);
        if (skipButton != null) skipButton.onClick.AddListener(SkipAllDialogue);

        // Ẩn các panel lúc đầu
        if (fatherPanel     != null) fatherPanel.SetActive(false);
        if (playerPanel     != null) playerPanel.SetActive(false);
        if (completionPanel != null) completionPanel.SetActive(false);
        SetButtonsVisible(false);

        // Nếu đã nói chuyện rồi → hiện panel hoàn thành luôn
        if (GameManager.Instance != null &&
            GameManager.Instance.chapter2State.dialogueCompleted)
        {
            if (talkToFatherNotice != null) talkToFatherNotice.SetActive(false);
            if (fatherButton       != null) fatherButton.SetActive(false);
            if (completionPanel    != null) completionPanel.SetActive(true);
        }
    }

    // ─────────────────────────────────────────────────────
    /// <summary>
    /// Gán hàm này vào BtnFather.OnClick() trong Inspector.
    /// </summary>
    public void StartDialogue()
    {
        if (_dialogueStarted) return;
        _dialogueStarted = true;

        // Ẩn thông báo và button click ba
        if (talkToFatherNotice != null) talkToFatherNotice.SetActive(false);
        if (fatherButton       != null) fatherButton.SetActive(false);

        // Hiện nút điều khiển hội thoại
        SetButtonsVisible(true);

        _dialogueIndex = 0;
        ShowCurrentLine();
    }

    // ─────────────────────────────────────────────────────
    private void ShowCurrentLine()
    {
        if (_dialogueIndex >= dialogueLines.Count)
        {
            EndChapter();
            return;
        }

        FatherDialogueLine line = dialogueLines[_dialogueIndex];
        Debug.Log($"[FatherDialogueTrigger] Dòng { _dialogueIndex}: Người nói = {line.speaker}, Nội dung = \"{line.content}\"");

        if (line.speaker == FatherSpeaker.Father)
        {
            if (fatherPanel != null)
            {
                fatherPanel.SetActive(true);
            }
            else
            {
                Debug.LogError("[FatherDialogueTrigger] Chưa gán 'Father Panel' trong Inspector!", this);
            }

            if (playerPanel != null) playerPanel.SetActive(false);

            if (fatherDialogueText != null)
            {
                fatherDialogueText.text = line.content;
                Debug.Log($"[FatherDialogueTrigger] Đã hiển thị văn bản cho Father: {line.content}");
            }
            else
            {
                Debug.LogError("[FatherDialogueTrigger] Chưa gán 'Father Dialogue Text' (TMP_Text) trong Inspector!", this);
            }
        }
        else
        {
            if (playerPanel != null)
            {
                playerPanel.SetActive(true);
            }
            else
            {
                Debug.LogError("[FatherDialogueTrigger] Chưa gán 'Player Panel' trong Inspector!", this);
            }

            if (fatherPanel != null) fatherPanel.SetActive(false);

            if (playerDialogueText != null)
            {
                playerDialogueText.text = line.content;
                Debug.Log($"[FatherDialogueTrigger] Đã hiển thị văn bản cho Player: {line.content}");
            }
            else
            {
                Debug.LogError("[FatherDialogueTrigger] Chưa gán 'Player Dialogue Text' (TMP_Text) trong Inspector!", this);
            }
        }
    }

    // ─────────────────────────────────────────────────────
    public void ShowNextLine()
    {
        _dialogueIndex++;
        ShowCurrentLine();
    }

    public void SkipAllDialogue()
    {
        _dialogueIndex = dialogueLines.Count;
        if (fatherPanel != null) fatherPanel.SetActive(false);
        if (playerPanel != null) playerPanel.SetActive(false);
        SetButtonsVisible(false);
        EndChapter();
    }

    // ─────────────────────────────────────────────────────
    private void EndChapter()
    {
        Debug.Log("[FatherDialogueTrigger] Chapter 2 hoàn thành!");

        if (fatherPanel != null) fatherPanel.SetActive(false);
        if (playerPanel != null) playerPanel.SetActive(false);
        SetButtonsVisible(false);

        // Hiện panel hoàn thành
        if (completionPanel != null) completionPanel.SetActive(true);

        // ── Lưu vào GameManager ──
        if (GameManager.Instance != null)
        {
            GameManager.Instance.HealFather();
        }

        // Thông báo FatherRoomManager hiện nút quay lại
        if (roomManager != null) roomManager.OnChapterCompleted();
    }

    // ─────────────────────────────────────────────────────
    private void SetButtonsVisible(bool visible)
    {
        if (nextButton != null) nextButton.gameObject.SetActive(visible);
        if (skipButton != null) skipButton.gameObject.SetActive(visible);
    }

    // ─────────────────────────────────────────────────────
    private List<FatherDialogueLine> GetDefaultDialogue()
    {
        return new List<FatherDialogueLine>
        {
            
        };
    }
}

// ── Enum người nói ────────────────────────────────────────
public enum FatherSpeaker { Father, Player }

/// <summary>Một dòng đối thoại trong cuộc hội thoại với ba.</summary>
[System.Serializable]
public class FatherDialogueLine
{
    [Tooltip("Người nói: Father hoặc Player")]
    public FatherSpeaker speaker;

    [TextArea(2, 5)]
    [Tooltip("Nội dung lời thoại")]
    public string content;

    public FatherDialogueLine(FatherSpeaker speaker, string content)
    {
        this.speaker = speaker;
        this.content = content;
    }
}
