using System.Collections;
using UnityEngine;

/// <summary>
/// Luồng sau khi ghép ảnh hoàn chỉnh:
/// 1. Hiện thông báo "Ghép xong!" → Hiện thông báo "Chạm để lấy bức ảnh"
/// 2. Người chơi chạm → Lấy bức ảnh vào túi đồ, chuyển sang Panel tường trống
/// 3. Nhấn Quay lại → Vào Panel_BrightRoom + hiện thông báo "Hãy tới nói chuyện với bà"
/// 4. Người chơi nhấn vào giường bà → Đối thoại (do GrandmaDialogueTrigger.cs xử lý)
/// </summary>
public class PuzzleCompleteHandler : MonoBehaviour
{
    [SerializeField] private MemoryPuzzle memoryPuzzle;

    // ── Thông báo sau khi ghép xong ──────────────────────
    [Header("Thông báo sau khi ghép xong")]
    [Tooltip("Panel thông báo 'Ghép ảnh thành công!'")]
    [SerializeField] private GameObject successNoticePanel;

    [Tooltip("Panel/Button 'Chạm để lấy bức ảnh' (hiện sau thông báo thành công)")]
    [SerializeField] private GameObject pickupPromptPanel;

    // ── Khung tranh hoàn chỉnh ───────────────────────────
    [Header("Bức ảnh để nhặt")]
    [Tooltip("Sprite hiển thị trong túi đồ sau khi nhặt khung")]
    [SerializeField] private Sprite completedFrameSprite;

    [Tooltip("ID vật phẩm khung tranh trong túi đồ")]
    [SerializeField] private string frameItemId = "CompletedFrame";

    // ── Các Panel chuyển cảnh ────────────────────────────
    [Header("Chuyển cảnh")]
    [Tooltip("Panel ghép ảnh hiện tại (sẽ ẩn khi lấy ảnh)")]
    [SerializeField] private GameObject frameAssemblyPanel;

    [Tooltip("Panel tường trống không còn bức ảnh (sau khi lấy)")]
    [SerializeField] private GameObject emptyWallPanel;

    [Tooltip("Panel phòng sáng chính (MainRoomViewBright) - Bước trung gian trước hội thoại")]
    [SerializeField] private GameObject brightRoomPanel;

    // ── Thông báo trong phòng sáng ───────────────────────
    [Header("Thông báo trong phòng sáng")]
    [Tooltip("Text/Panel thông báo 'Hãy tới nói chuyện với bà' trong Panel_BrightRoom")]
    [SerializeField] private GameObject talkToGrandmaNotice;

    [Header("Music")]
    [SerializeField] private ChapterMusicStatePlayer chapterMusic;

    // ── Biến trạng thái ──────────────────────────────────
    private bool _framePickedUp = false;

    // ─────────────────────────────────────────────────────
    private void Start()
    {
        if (memoryPuzzle != null)
            memoryPuzzle.OnPuzzleCompleted += OnPuzzleCompleted;

        // Ẩn tất cả các UI chưa cần thiết lúc đầu
        if (successNoticePanel != null)  successNoticePanel.SetActive(false);
        if (pickupPromptPanel != null)   pickupPromptPanel.SetActive(false);
        if (emptyWallPanel != null)      emptyWallPanel.SetActive(false);
        if (brightRoomPanel != null)     brightRoomPanel.SetActive(false);
        if (talkToGrandmaNotice != null) talkToGrandmaNotice.SetActive(false);

        // ── Memento: Khôi phục trạng thái từ GameManager ──
        RestoreFromGameManager();
    }

    /// <summary>
    /// Khôi phục trạng thái PuzzleCompleteHandler từ GameManager khi Scene tải lại.
    /// </summary>
    private void RestoreFromGameManager()
    {
        if (GameManager.Instance == null) return;

        var state = GameManager.Instance.chapter1State;

        // Nếu đã nhặt khung tranh rồi: hiện phòng sáng + thông báo nói chuyện với bà
        if (state.framePickedUp)
        {
            _framePickedUp = true;
            if (frameAssemblyPanel != null) frameAssemblyPanel.SetActive(false);
            if (emptyWallPanel != null)     emptyWallPanel.SetActive(false);
            if (brightRoomPanel != null)    brightRoomPanel.SetActive(true);

            // Nếu hội thoại chưa hoàn thành thì tiếp tục hiện thông báo
            if (!state.dialogueCompleted && talkToGrandmaNotice != null)
                talkToGrandmaNotice.SetActive(true);

            Debug.Log("[PuzzleCompleteHandler] Khôi phục: khung tranh đã được nhặt.");
        }
    }

    private void OnDestroy()
    {
        if (memoryPuzzle != null)
            memoryPuzzle.OnPuzzleCompleted -= OnPuzzleCompleted;
    }

    // ─── Khi MemoryPuzzle báo hoàn thành ─────────────────
    private void OnPuzzleCompleted()
    {
        StartCoroutine(ShowSuccessSequence());
    }

    /// Hiện thông báo thành công → rồi hiện nút "Chạm để lấy"
    private IEnumerator ShowSuccessSequence()
    {
        // Bước 1: Hiện thông báo "Ghép ảnh thành công!"
        if (successNoticePanel != null)
        {
            successNoticePanel.SetActive(true);
        }

        // Đợi 2 giây để người chơi đọc thông báo
        yield return new WaitForSeconds(2f);

        // Ẩn thông báo thành công
        if (successNoticePanel != null)
        {
            successNoticePanel.SetActive(false);
        }

        // Bước 2: Hiện nút/thông báo "Chạm để lấy bức ảnh"
        if (pickupPromptPanel != null)
        {
            pickupPromptPanel.SetActive(true);
        }
    }

    // ─── Người chơi chạm "Chạm để lấy bức ảnh" ──────────
    // Gắn hàm này vào sự kiện OnClick của pickupPromptPanel (Button)
    public void PickUpCompletedFrame()
    {
        if (_framePickedUp) return;

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("[PuzzleCompleteHandler] Không tìm thấy InventoryManager!");
            return;
        }

        bool collected = InventoryManager.Instance.CollectItem(frameItemId, completedFrameSprite);

        if (collected)
        {
            _framePickedUp = true;
            Debug.Log("[PuzzleCompleteHandler] Đã nhặt khung tranh vào túi đồ!");

            // ── Memento: Lưu vào GameManager ──
            if (GameManager.Instance != null)
                GameManager.Instance.chapter1State.framePickedUp = true;

            // Ẩn nút "Chạm để lấy"
            if (pickupPromptPanel != null) pickupPromptPanel.SetActive(false);

            // Chuyển sang Panel tường trống (không còn ảnh trên tường)
            if (frameAssemblyPanel != null) frameAssemblyPanel.SetActive(false);
            if (emptyWallPanel != null)     emptyWallPanel.SetActive(true);
        }
    }

    // ─── Người chơi nhấn Quay lại từ Panel tường trống ──
    // Gắn hàm này vào sự kiện OnClick của nút "Quay lại" trên emptyWallPanel
    public void GoBackToBrightRoom()
    {
        // Ẩn Panel tường trống
        if (emptyWallPanel != null) emptyWallPanel.SetActive(false);

        // Hiện phòng sáng chính
        if (brightRoomPanel != null) brightRoomPanel.SetActive(true);

        // Hiện thông báo "Hãy tới nói chuyện với bà"
        if (talkToGrandmaNotice != null)
        {
            talkToGrandmaNotice.SetActive(true);
        }

        if (chapterMusic != null)
        {
            chapterMusic.PlayHealedMusic();
        }
        else
        {
            Debug.LogWarning(
                "[PuzzleCompleteHandler] Chapter Music chưa được gán; phòng sáng vẫn dùng nhạc cũ.",
                this);
        }
    }
}
