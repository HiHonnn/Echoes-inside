using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Xử lý cuộc hội thoại khi người chơi nhấn vào giường bà.
/// Hỗ trợ 2 panel nhân vật riêng biệt (bà và người chơi), nút Tiếp theo và Bỏ qua.
/// </summary>
public class GrandmaDialogueTrigger : MonoBehaviour
{
    [Header("Thông báo 'Hãy nói chuyện với bà' (sẽ ẩn khi bắt đầu thoại)")]
    [SerializeField] private GameObject talkToGrandmaNotice;

    // ── 2 Panel nhân vật ─────────────────────────────────
    [Header("Panel nhân vật BÀ (hiện khi bà nói)")]
    [Tooltip("Toàn bộ Panel chứa ảnh bà + hộp thoại bà")]
    [SerializeField] private GameObject grandmaPanel;
    [Tooltip("Text hiển thị lời thoại của bà")]
    [SerializeField] private TMP_Text grandmaDialogueText;

    [Header("Panel nhân vật NGƯỜI CHƠI (hiện khi người chơi nói)")]
    [Tooltip("Toàn bộ Panel chứa ảnh người chơi + hộp thoại người chơi")]
    [SerializeField] private GameObject playerPanel;
    [Tooltip("Text hiển thị lời thoại của người chơi")]
    [SerializeField] private TMP_Text playerDialogueText;

    // ── Nút điều khiển ───────────────────────────────────
    [Header("Nút điều khiển")]
    [SerializeField] private Button nextButton;
    [SerializeField] private Button skipButton;

    // ── Nội dung đối thoại ────────────────────────────────
    [Header("Nội dung đối thoại")]
    [SerializeField] private List<DialogueLine> dialogueLines;

    // ── Kết thúc màn ─────────────────────────────────────
    [Header("Kết thúc chương 1")]
    [Tooltip("Panel bà đang nhìn bức ảnh (hiện sau hội thoại & mỗi lần vào lại phòng)")]
    [SerializeField] private GameObject grandmaWithPhotoPanel;

    [Tooltip("Panel phòng bà tối (MainRoomView - hiện ban đầu)")]
    [SerializeField] private GameObject darkRoomPanel;

    [Tooltip("Panel phòng bà sáng (MainRoomViewBright)")]
    [SerializeField] private GameObject brightRoomPanel;

    // ── Trạng thái ───────────────────────────────────────
    private int _dialogueIndex = 0;
    private bool _dialogueStarted = false;

    // ─────────────────────────────────────────────────────
    private void Awake()
    {
        // Nạp sẵn nội dung đối thoại nếu chưa có
        if (dialogueLines == null || dialogueLines.Count == 0)
        {
            dialogueLines = GetDefaultDialogue();
        }
    }

    private void Start()
    {
        if (nextButton != null) nextButton.onClick.AddListener(ShowNextLine);
        if (skipButton != null) skipButton.onClick.AddListener(SkipAllDialogue);

        if (grandmaPanel != null)       grandmaPanel.SetActive(false);
        if (playerPanel != null)        playerPanel.SetActive(false);
        if (grandmaWithPhotoPanel != null) grandmaWithPhotoPanel.SetActive(false);

        // Ẩn nút khi chưa bắt đầu thoại
        SetButtonsVisible(false);

        // ── Memento: Nếu đã chữa lành bà rồi → hiện panel bà nhìn ảnh luôn ──
        RestoreFromGameManager();
    }

    /// <summary>
    /// Nếu bà đã được chữa lành (dialogueCompleted), ẩn phòng tối/sáng
    /// và hiện thẳng panel bà đang nhìn bức ảnh.
    /// </summary>
    private void RestoreFromGameManager()
    {
        if (GameManager.Instance == null) return;

        if (GameManager.Instance.chapter1State.dialogueCompleted)
        {
            // Ẩn phòng tối và phòng sáng
            if (darkRoomPanel != null)   darkRoomPanel.SetActive(false);
            if (brightRoomPanel != null) brightRoomPanel.SetActive(false);

            // Hiện panel bà đang nhìn bức ảnh
            if (grandmaWithPhotoPanel != null) grandmaWithPhotoPanel.SetActive(true);

            Debug.Log("[GrandmaDialogueTrigger] Khôi phục: Bà đã được chữa lành, hiện panel bà nhìn ảnh.");
        }
    }

    // ─── Người chơi nhấn vào giường bà ──────────────────
    public void TriggerDialogue()
    {
        if (_dialogueStarted) return;
        _dialogueStarted = true;

        if (talkToGrandmaNotice != null)
            talkToGrandmaNotice.SetActive(false);

        // Hiện nút khi bắt đầu thoại
        SetButtonsVisible(true);

        _dialogueIndex = 0;
        ShowCurrentLine();
    }

    // ─── Hiện dòng thoại hiện tại ────────────────────────
    private void ShowCurrentLine()
    {
        if (_dialogueIndex >= dialogueLines.Count)
        {
            EndChapter();
            return;
        }

        DialogueLine line = dialogueLines[_dialogueIndex];

        if (line.speaker == Speaker.Grandma)
        {
            // Hiện panel bà, ẩn panel người chơi
            if (grandmaPanel != null)  grandmaPanel.SetActive(true);
            if (playerPanel != null)   playerPanel.SetActive(false);
            if (grandmaDialogueText != null) grandmaDialogueText.text = line.content;
        }
        else
        {
            // Hiện panel người chơi, ẩn panel bà
            if (playerPanel != null)   playerPanel.SetActive(true);
            if (grandmaPanel != null)  grandmaPanel.SetActive(false);
            if (playerDialogueText != null) playerDialogueText.text = line.content;
        }
    }

    // ─── Nút "Tiếp theo" ────────────────────────────────
    public void ShowNextLine()
    {
        _dialogueIndex++;
        ShowCurrentLine();
    }

    // ─── Nút "Bỏ qua" ───────────────────────────────────
    public void SkipAllDialogue()
    {
        _dialogueIndex = dialogueLines.Count;
        if (grandmaPanel != null) grandmaPanel.SetActive(false);
        if (playerPanel != null)  playerPanel.SetActive(false);
        SetButtonsVisible(false);
        EndChapter();
    }

    // ─── Kết thúc chương ────────────────────────────────
    private void EndChapter()
    {
        Debug.Log("[GrandmaDialogueTrigger] Chương 1 hoàn thành!");

        if (grandmaPanel != null) grandmaPanel.SetActive(false);
        if (playerPanel != null)  playerPanel.SetActive(false);

        // Ẩn nút khi kết thúc thoại
        SetButtonsVisible(false);

        // Ẩn phòng sáng (nếu đang hiện)
        if (brightRoomPanel != null) brightRoomPanel.SetActive(false);

        // Hiện panel bà đang nhìn bức ảnh
        if (grandmaWithPhotoPanel != null) grandmaWithPhotoPanel.SetActive(true);

        // ── Memento: Lưu trạng thái hoàn thành vào GameManager ──
        if (GameManager.Instance != null)
        {
            GameManager.Instance.HealGrandma();
        }
    }

    // ─── Bật/tắt nút điều khiển ─────────────────────────
    private void SetButtonsVisible(bool visible)
    {
        if (nextButton != null) nextButton.gameObject.SetActive(visible);
        if (skipButton != null) skipButton.gameObject.SetActive(visible);
    }

    // ─── Nội dung đối thoại mặc định ────────────────────
    private List<DialogueLine> GetDefaultDialogue()
    {
        return new List<DialogueLine>
        {
            new DialogueLine(Speaker.Player,  "Bà ơi... con tìm thấy bức ảnh này trong phòng. Bà có muốn nhận lại không?"),
            new DialogueLine(Speaker.Grandma, "Ôi trời... bức ảnh của ông bà. Con tìm được ở đâu vậy?"),
            new DialogueLine(Speaker.Player,  "Con nhặt được các mảnh vỡ từ trong phòng rồi ghép lại. Có vẻ nó đã bị vỡ từ lâu rồi."),
            new DialogueLine(Speaker.Grandma, "Ừ... ông con vô tình làm rơi vỡ trước khi ông mất. Bà cứ nghĩ không còn tìm lại được nữa..."),
            new DialogueLine(Speaker.Grandma, "Cảm ơn con nhiều lắm. Đây là bức ảnh duy nhất bà còn có với ông."),
            new DialogueLine(Speaker.Player,  "Bà có nhớ ông nhiều không ạ?"),
            new DialogueLine(Speaker.Grandma, "Mỗi đêm bà đều nghĩ về ông. Nhà này... từ khi ông mất, sao mà rộng và lạnh lẽo quá."),
            new DialogueLine(Speaker.Grandma, "Các con các cháu đều bận rộn với cuộc sống riêng. Bà hiểu... nhưng đôi khi bà cũng cô đơn lắm."),
            new DialogueLine(Speaker.Player,  "Con... con không biết bà cảm thấy như vậy. Con xin lỗi vì ít về thăm bà."),
            new DialogueLine(Speaker.Grandma, "Không sao đâu con. Hôm nay con về đây, ngồi lại cùng bà, bà đã vui hơn rất nhiều rồi."),
            new DialogueLine(Speaker.Grandma, "Có bức ảnh này, bà cảm giác như ông vẫn còn ở bên cạnh bà. Cảm ơn con... cảm ơn con nhiều lắm."),
            new DialogueLine(Speaker.Player,  "Con sẽ thường xuyên về thăm bà hơn. Con hứa."),
            new DialogueLine(Speaker.Grandma, "(Mỉm cười) Bà tin con mà. Thôi con về nghỉ ngơi đi nhé, bà ổn rồi."),
        };
    }
}

// ── Enum xác định người nói ──────────────────────────────
public enum Speaker { Grandma, Player }

/// <summary>
/// Một dòng đối thoại gồm người nói và nội dung.
/// </summary>
[System.Serializable]
public class DialogueLine
{
    [Tooltip("Người nói: Grandma hoặc Player")]
    public Speaker speaker;

    [TextArea(2, 5)]
    [Tooltip("Nội dung lời thoại")]
    public string content;

    public DialogueLine(Speaker speaker, string content)
    {
        this.speaker = speaker;
        this.content = content;
    }
}
