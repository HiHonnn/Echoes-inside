using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Xử lý cuộc hội thoại khi người chơi nhấn vào mẹ sau khi hoàn thành màn bếp.
/// Cấu trúc giống GrandmaDialogueTrigger của Chapter 1.
/// Sau khi hội thoại kết thúc:
///   - Lưu chapter3State.dialogueCompleted = true
///   - Lưu isMotherHealed = true vào GameManager
///   - Hiện panel mẹ đã được chữa lành
/// </summary>
public class MotherDialogueTrigger : MonoBehaviour
{
    [Header("Thông báo 'Hãy nói chuyện với mẹ' (ẩn khi bắt đầu thoại)")]
    [SerializeField] private GameObject promptTalkToMom;

    // ── 2 Panel nhân vật ──────────────────────────────────────
    [Header("Panel nhân vật MẸ (hiện khi mẹ nói)")]
    [SerializeField] private GameObject momPanel;
    [SerializeField] private TMP_Text momDialogueText;

    [Header("Panel nhân vật NGƯỜI CHƠI (hiện khi người chơi nói)")]
    [SerializeField] private GameObject playerPanel;
    [SerializeField] private TMP_Text playerDialogueText;

    // ── Nút điều khiển ────────────────────────────────────────
    [Header("Nút điều khiển")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button skipButton;

    // ── Nội dung đối thoại ────────────────────────────────────
    [Header("Nội dung đối thoại (để trống = dùng mặc định)")]
    [SerializeField] private List<MotherDialogueLine> dialogueLines;

    // ── Kết thúc Chapter 3 ────────────────────────────────────
    [Header("Kết thúc Chapter 3")]
    [Tooltip("Panel mẹ đã thức — sẽ ẩn khi hội thoại bắt đầu")]
    [SerializeField] private GameObject panelMomAwake;

    [Tooltip("Panel mẹ đã được chữa lành — hiện sau hội thoại")]
    [SerializeField] private GameObject panelMomHealed;

    // ── Trạng thái ────────────────────────────────────────────
    private int _dialogueIndex = 0;
    private bool _dialogueStarted = false;

    // ─────────────────────────────────────────────────────────
    private void Awake()
    {
        if (dialogueLines == null || dialogueLines.Count == 0)
            dialogueLines = GetDefaultDialogue();
    }

    private void Start()
    {
        if (nextButton != null) nextButton.onClick.AddListener(ShowNextLine);
        if (skipButton != null) skipButton.onClick.AddListener(SkipAllDialogue);

        if (momPanel != null)    momPanel.SetActive(false);
        if (playerPanel != null) playerPanel.SetActive(false);
        SetButtonsVisible(false);

        // Nếu đã hoàn thành hội thoại → hiện panel mẹ đã lành luôn
        RestoreFromGameManager();
    }

    private void RestoreFromGameManager()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.chapter3State.dialogueCompleted)
        {
            if (panelMomAwake != null)  panelMomAwake.SetActive(false);
            if (panelMomHealed != null) panelMomHealed.SetActive(true);
            Debug.Log("[MotherDialogueTrigger] Khôi phục: Mẹ đã được chữa lành.");
        }
    }

    // ── Bắt đầu hội thoại (gọi từ MomRoomManager) ────────────
    public void TriggerDialogue()
    {
        if (_dialogueStarted) return;
        _dialogueStarted = true;

        if (promptTalkToMom != null) promptTalkToMom.SetActive(false);
        SetButtonsVisible(true);

        _dialogueIndex = 0;
        ShowCurrentLine();
    }

    // ── Hiện dòng thoại hiện tại ─────────────────────────────
    private void ShowCurrentLine()
    {
        if (_dialogueIndex >= dialogueLines.Count)
        {
            EndChapter();
            return;
        }

        MotherDialogueLine line = dialogueLines[_dialogueIndex];

        if (line.speaker == MotherSpeaker.Mother)
        {
            if (momPanel != null)       momPanel.SetActive(true);
            if (playerPanel != null)    playerPanel.SetActive(false);
            if (momDialogueText != null) momDialogueText.text = line.content;
        }
        else
        {
            if (playerPanel != null)        playerPanel.SetActive(true);
            if (momPanel != null)           momPanel.SetActive(false);
            if (playerDialogueText != null) playerDialogueText.text = line.content;
        }
    }

    public void ShowNextLine()
    {
        _dialogueIndex++;
        ShowCurrentLine();
    }

    public void SkipAllDialogue()
    {
        _dialogueIndex = dialogueLines.Count;
        if (momPanel != null)    momPanel.SetActive(false);
        if (playerPanel != null) playerPanel.SetActive(false);
        SetButtonsVisible(false);
        EndChapter();
    }

    // ── Kết thúc Chapter 3 ───────────────────────────────────
    private void EndChapter()
    {
        Debug.Log("[MotherDialogueTrigger] ✅ Chapter 3 hoàn thành! Mẹ đã được chữa lành.");

        if (momPanel != null)    momPanel.SetActive(false);
        if (playerPanel != null) playerPanel.SetActive(false);
        SetButtonsVisible(false);

        // Ẩn panel mẹ thức, hiện panel mẹ đã lành
        if (panelMomAwake != null)  panelMomAwake.SetActive(false);
        if (panelMomHealed != null) panelMomHealed.SetActive(true);

        // ── Memento: Lưu vào GameManager ──────────────────────
        if (GameManager.Instance != null)
        {
            GameManager.Instance.HealMother();
        }
    }

    private void SetButtonsVisible(bool visible)
    {
        if (nextButton != null) nextButton.gameObject.SetActive(visible);
        if (skipButton != null) skipButton.gameObject.SetActive(visible);
    }

    // ── Nội dung hội thoại mặc định ──────────────────────────
    private List<MotherDialogueLine> GetDefaultDialogue()
    {
        return new List<MotherDialogueLine>
        {
            new MotherDialogueLine(MotherSpeaker.Mother, "(Mẹ từ từ mở mắt, nhìn quanh phòng với ánh mắt xa xăm)"),
            new MotherDialogueLine(MotherSpeaker.Player, "Mẹ ơi... mẹ vừa mơ gì vậy?"),
            new MotherDialogueLine(MotherSpeaker.Mother, "Mẹ mơ thấy mình đang nấu ăn... từng món một, cho mỗi người trong nhà."),
            new MotherDialogueLine(MotherSpeaker.Mother, "Mẹ nấu cháo cho bà, cơm sườn cho ba, trứng cuộn cho chị... và mì xào cho con."),
            new MotherDialogueLine(MotherSpeaker.Player, "Mẹ hay nhớ món ăn yêu thích của mỗi người lắm nhỉ..."),
            new MotherDialogueLine(MotherSpeaker.Mother, "Bởi vì mỗi bữa cơm, mẹ muốn mọi người cảm thấy được yêu thương qua từng món ăn."),
            new MotherDialogueLine(MotherSpeaker.Mother, "Nhưng rồi mọi người đều bận rộn, lớn lên, ít về nhà hơn..."),
            new MotherDialogueLine(MotherSpeaker.Mother, "Mẹ vẫn nấu, nhưng bàn ăn ngày càng ít người..."),
            new MotherDialogueLine(MotherSpeaker.Player, "Con... con không biết mẹ đã cô đơn như vậy. Con xin lỗi."),
            new MotherDialogueLine(MotherSpeaker.Mother, "(Mỉm cười nhẹ) Không sao đâu con. Hôm nay con ngồi lại đây với mẹ là mẹ vui rồi."),
            new MotherDialogueLine(MotherSpeaker.Mother, "Mẹ chỉ muốn con biết một điều — dù con ở đâu, bàn ăn này lúc nào cũng có chỗ cho con."),
            new MotherDialogueLine(MotherSpeaker.Player, "Mẹ... con sẽ về nhà thường xuyên hơn. Con hứa sẽ ăn cơm cùng mẹ."),
            new MotherDialogueLine(MotherSpeaker.Mother, "(Nắm tay con, mắt ướt) Mẹ chờ con. Lúc nào cũng chờ."),
        };
    }
}

public enum MotherSpeaker { Mother, Player }

[System.Serializable]
public class MotherDialogueLine
{
    [Tooltip("Người nói: Mother hoặc Player")]
    public MotherSpeaker speaker;

    [TextArea(2, 5)]
    public string content;

    public MotherDialogueLine(MotherSpeaker speaker, string content)
    {
        this.speaker = speaker;
        this.content = content;
    }
}
