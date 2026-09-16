using UnityEngine;

/// <summary>
/// Quản lý 2 trạng thái của phòng ba trong Chapter2_Father.
/// Panel_BeforeMaze: Ba gục trên bàn (trước khi hoàn thành mê cung).
/// Panel_AfterMaze:  Ba thức giấc (sau khi hoàn thành mê cung).
/// </summary>
public class FatherRoomManager : MonoBehaviour
{
    [Header("2 Panel Trạng Thái Phòng")]
    [Tooltip("Panel hiện khi mê cung CHƯA hoàn thành — ba gục trên bàn")]
    [SerializeField] private GameObject panelBeforeMaze;

    [Tooltip("Panel hiện khi mê cung ĐÃ hoàn thành — ba thức giấc")]
    [SerializeField] private GameObject panelAfterMaze;

    [Header("Nút Quay Lại (chỉ hiện sau khi hoàn thành chapter)")]
    [Tooltip("Nút quay về Map Chính — ẩn khi chưa nói chuyện với ba")]
    [SerializeField] private GameObject backButton;

    // ─────────────────────────────────────────────────────
    private void Start()
    {
        RefreshRoomState();
    }

    // ─────────────────────────────────────────────────────
    /// <summary>
    /// Gọi hàm này từ bất kỳ script nào để cập nhật lại trạng thái phòng.
    /// </summary>
    public void RefreshRoomState()
    {
        bool mazeCompleted     = GameManager.Instance != null &&
                                 GameManager.Instance.chapter2State.mazeCompleted;
        bool dialogueCompleted = GameManager.Instance != null &&
                                 GameManager.Instance.chapter2State.dialogueCompleted;

        // Bật đúng panel
        if (panelBeforeMaze == null)
        {
            Debug.LogError("[FatherRoomManager] Chưa gán Panel Before Maze trong Inspector!", this);
        }
        else
        {
            panelBeforeMaze.SetActive(!mazeCompleted);
        }

        if (panelAfterMaze == null)
        {
            Debug.LogError("[FatherRoomManager] Chưa gán Panel After Maze trong Inspector!", this);
        }
        else
        {
            panelAfterMaze.SetActive(mazeCompleted);
        }

        // Nút quay lại chỉ hiện khi đã nói chuyện xong
        if (backButton != null)
        {
            backButton.SetActive(dialogueCompleted);
        }
        else
        {
            Debug.LogWarning("[FatherRoomManager] Chưa gán Back Button trong Inspector!", this);
        }

        Debug.Log($"[FatherRoomManager] RefreshRoomState: mazeCompleted={mazeCompleted}, dialogueCompleted={dialogueCompleted}");
    }

    // ─────────────────────────────────────────────────────
    /// <summary>
    /// Gọi từ FatherDialogueTrigger khi hội thoại kết thúc.
    /// </summary>
    public void OnChapterCompleted()
    {
        // Hiện nút quay lại
        if (backButton != null) backButton.SetActive(true);
    }
}
