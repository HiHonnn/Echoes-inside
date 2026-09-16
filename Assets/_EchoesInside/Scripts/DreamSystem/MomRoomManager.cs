using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý trạng thái phòng ngủ của Mẹ (Chapter3_MomRoom).
/// Giống FatherRoomManager của Chapter 2:
///   - Lúc đầu: hiện cảnh mẹ đang ngủ (MomSleeping).
///   - Người chơi click vào mẹ → vào Scene bếp nấu ăn.
///   - Sau khi thắng màn bếp (chapter3State.mealCompleted == true):
///     hiện cảnh mẹ thức dậy (MomAwake), người chơi click để hội thoại.
/// </summary>
public class MomRoomManager : MonoBehaviour
{
    // ── Các Panel cảnh phòng ngủ ───────────────────────────────
    [Header("Panels cảnh phòng")]
    [Tooltip("Ảnh phòng + mẹ đang ngủ — hiện khi chưa hoàn thành bếp")]
    [SerializeField] private GameObject panelMomSleeping;

    [Tooltip("Ảnh phòng + mẹ thức — hiện sau khi hoàn thành bếp")]
    [SerializeField] private GameObject panelMomAwake;

    [Tooltip("Ảnh phòng sau hội thoại kết thúc (mẹ đã được chữa lành)")]
    [SerializeField] private GameObject panelMomHealed;

    // ── Gợi ý tương tác ───────────────────────────────────────
    [Header("Gợi ý tương tác")]
    [Tooltip("Text 'Chạm vào mẹ để vào giấc mơ' — hiện khi mẹ đang ngủ")]
    [SerializeField] private GameObject promptEnterDream;

    [Tooltip("Text 'Chạm vào mẹ để nói chuyện' — hiện khi mẹ đã thức")]
    [SerializeField] private GameObject promptTalkToMom;

    // ── Cài đặt ───────────────────────────────────────────────
    [Header("Cài đặt Scene")]
    [SerializeField] private string kitchenSceneName = "Chapter3_Kitchen";

    private void OnValidate()
    {
        if (panelMomSleeping == null)
            Debug.LogWarning("[MomRoomManager] Chưa gán Panel Mom Sleeping.", this);

        if (panelMomAwake == null)
            Debug.LogWarning("[MomRoomManager] Chưa gán Panel Mom Awake.", this);

        if (panelMomHealed == null)
            Debug.LogWarning("[MomRoomManager] Chưa gán Panel Mom Healed.", this);

        if (promptEnterDream == null)
            Debug.LogWarning("[MomRoomManager] Chưa gán Prompt Enter Dream.", this);

        if (promptTalkToMom == null)
            Debug.LogWarning("[MomRoomManager] Chưa gán Prompt Talk To Mom.", this);

        if (string.IsNullOrWhiteSpace(kitchenSceneName))
            Debug.LogWarning("[MomRoomManager] Kitchen Scene Name đang trống.", this);
    }

    // ─────────────────────────────────────────────────────────
    private void Start()
    {
        // Ẩn tất cả trước
        SetAllPanels(false);

        // Căn cứ trạng thái GameManager để quyết định hiện panel nào
        RefreshRoomState();
    }

    /// <summary>
    /// Được gọi trong Start() và bởi FatherRoomEnvironment khi factory khởi tạo.
    /// Đọc GameManager để quyết định trạng thái phòng.
    /// </summary>
    public void RefreshRoomState()
    {
        if (GameManager.Instance == null)
        {
            // Fallback: chưa có GameManager → mặc định hiện mẹ đang ngủ
            ShowSleepingState();
            return;
        }

        var state = GameManager.Instance.chapter3State;

        if (state.dialogueCompleted)
        {
            // Mẹ đã được chữa lành hoàn toàn
            ShowHealedState();
        }
        else if (state.mealCompleted)
        {
            // Đã nấu xong → mẹ thức, chờ hội thoại
            ShowAwakeState();
        }
        else
        {
            // Chưa làm gì → mẹ ngủ
            ShowSleepingState();
        }
    }

    // ── Trạng thái 1: Mẹ đang ngủ ────────────────────────────
    private void ShowSleepingState()
    {
        if (panelMomSleeping != null) panelMomSleeping.SetActive(true);
        if (panelMomAwake != null)    panelMomAwake.SetActive(false);
        if (panelMomHealed != null)   panelMomHealed.SetActive(false);
        if (promptEnterDream != null) promptEnterDream.SetActive(true);
        if (promptTalkToMom != null)  promptTalkToMom.SetActive(false);
        Debug.Log("[MomRoomManager] Hiện cảnh mẹ đang ngủ.");
    }

    // ── Trạng thái 2: Mẹ đã thức (sau khi thắng bếp) ─────────
    private void ShowAwakeState()
    {
        if (panelMomSleeping != null) panelMomSleeping.SetActive(false);
        if (panelMomAwake != null)    panelMomAwake.SetActive(true);
        if (panelMomHealed != null)   panelMomHealed.SetActive(false);
        if (promptEnterDream != null) promptEnterDream.SetActive(false);
        if (promptTalkToMom != null)  promptTalkToMom.SetActive(true);
        Debug.Log("[MomRoomManager] Hiện cảnh mẹ đã thức dậy, chờ hội thoại.");
    }

    // ── Trạng thái 3: Mẹ đã được chữa lành ────────────────────
    private void ShowHealedState()
    {
        if (panelMomSleeping != null) panelMomSleeping.SetActive(false);
        if (panelMomAwake != null)    panelMomAwake.SetActive(false);
        if (panelMomHealed != null)   panelMomHealed.SetActive(true);
        if (promptEnterDream != null) promptEnterDream.SetActive(false);
        if (promptTalkToMom != null)  promptTalkToMom.SetActive(false);
        Debug.Log("[MomRoomManager] Hiện cảnh mẹ đã được chữa lành.");
    }

    // ── Người chơi nhấn vào mẹ đang ngủ ──────────────────────
    /// <summary>Gắn hàm này vào Button trên panelMomSleeping.</summary>
    public void OnClickMomSleeping()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.chapter3State.mealCompleted)
        {
            // Nếu bếp đã xong, đừng load lại
            Debug.Log("[MomRoomManager] Bếp đã hoàn thành, hãy nói chuyện với mẹ.");
            return;
        }

        Debug.Log("[MomRoomManager] Vào giấc mơ bếp của mẹ!");
        SceneTransitionLoader.LoadScene(kitchenSceneName, this);
    }

    // ── Người chơi nhấn vào mẹ đã thức ───────────────────────
    /// <summary>Gắn hàm này vào Button trên panelMomAwake.</summary>
    public void OnClickMomAwake()
    {
        // Kích hoạt hệ thống hội thoại
        var dialogue = FindFirstObjectByType<MotherDialogueTrigger>();
        if (dialogue != null)
        {
            dialogue.TriggerDialogue();
        }
        else
        {
            Debug.LogWarning("[MomRoomManager] Không tìm thấy MotherDialogueTrigger trong Scene!");
        }
    }

    // ── Tiện ích ──────────────────────────────────────────────
    private void SetAllPanels(bool active)
    {
        if (panelMomSleeping != null) panelMomSleeping.SetActive(active);
        if (panelMomAwake != null)    panelMomAwake.SetActive(active);
        if (panelMomHealed != null)   panelMomHealed.SetActive(active);
        if (promptEnterDream != null) promptEnterDream.SetActive(active);
        if (promptTalkToMom != null)  promptTalkToMom.SetActive(active);
    }
}
