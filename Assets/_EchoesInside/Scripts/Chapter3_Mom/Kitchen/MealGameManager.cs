using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Singleton quản lý toàn bộ gameplay Chapter 3 — "Bữa Cơm Của Mẹ".
///
/// Trách nhiệm:
///   - Đếm ngược 3 phút (bộ đếm thời gian).
///   - Theo dõi số bếp đã hoàn thành.
///   - Phát sự kiện OnAllStationsCompleted khi đủ 4 bếp (Observer Pattern).
///   - Kích hoạt màn hình Thắng / Thua.
///   - Việc lưu tiến trình được Observer bên ngoài xử lý.
/// </summary>
public class MealGameManager : MonoBehaviour
{
    public static MealGameManager Instance { get; private set; }

    // ── Cấu hình ──────────────────────────────────────────────
    [Header("Cấu hình")]
    [SerializeField] private float totalTime = 180f;   // 3 phút
    [SerializeField] private int totalStations = 4;

    [Tooltip("Tên Scene phòng ngủ mẹ để quay về sau khi kết thúc")]
    [SerializeField] private string momRoomScene = "Chapter3_MomRoom";

    // ── UI ────────────────────────────────────────────────────
    [Header("UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    [Tooltip("Panel thông báo hiện khi bắt đầu game, tự ẩn sau noticeDisplayTime giây")]
    [SerializeField] private GameObject noticePanel;
    [SerializeField] private float noticeDisplayTime = 10f;

    // ── Tham chiếu ────────────────────────────────────────────
    [Header("References")]
    [SerializeField] private IngredientSpawner spawner;

    // ── Observer: Sự kiện khi TẤT CẢ 4 bếp hoàn thành ───────
    /// <summary>
    /// Đăng ký lắng nghe sự kiện này để xử lý cutscene/dialogue kết thúc.
    /// Ví dụ: MealPuzzleStrategy.HandleMealCompleted
    /// </summary>
    public event Action OnAllStationsCompleted;

    // ── Trạng thái nội bộ ─────────────────────────────────────
    private float _timeRemaining;
    private int _completedCount = 0;
    private bool _gameEnded = false;
    private List<string> _completedStationIds = new();

    // ─────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        _timeRemaining = totalTime;
        _completedCount = 0;
        _gameEnded = false;
        _completedStationIds.Clear();

        // Mỗi lần vào scene bếp là một lượt chơi mới. Chỉ kết quả thắng
        // cuối cùng (mealCompleted) mới được giữ xuyên scene.
        if (GameManager.Instance != null)
            GameManager.Instance.BeginMotherMealAttempt();

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        // Hiện Notice 10 giây rồi tự ẩn
        if (noticePanel != null)
        {
            noticePanel.SetActive(true);
            StartCoroutine(HideNoticeAfterDelay());
        }

        UpdateTimerUI();

        // Bắt đầu spawn nguyên liệu
        spawner?.StartSpawning();
    }

    private IEnumerator HideNoticeAfterDelay()
    {
        yield return new WaitForSeconds(noticeDisplayTime);
        if (noticePanel != null)
            noticePanel.SetActive(false);
    }

    private void Update()
    {
        if (_gameEnded) return;

        _timeRemaining -= Time.deltaTime;
        UpdateTimerUI();

        if (_timeRemaining <= 0f)
        {
            _timeRemaining = 0f;
            UpdateTimerUI();
            TriggerLose();
        }
    }

    // ── Nhận thông báo từ CookingStation ─────────────────────
    /// <summary>
    /// Được gọi bởi CookingStation khi một bếp hoàn thành.
    /// </summary>
    public void OnStationCompleted(string stationId)
    {
        if (_completedStationIds.Contains(stationId)) return;
        _completedStationIds.Add(stationId);
        _completedCount++;

        // Lưu vào GameManager
        if (GameManager.Instance != null)
            GameManager.Instance.SaveMotherStationCompleted(stationId);

        Debug.Log($"[MealGameManager] Bếp '{stationId}' ✅ ({_completedCount}/{totalStations})");

        if (_completedCount >= totalStations)
            TriggerWin();
    }

    // ── Thắng ───────────────────────────────────────────
    private void TriggerWin()
    {
        if (_gameEnded) return;
        _gameEnded = true;

        spawner?.StopSpawning();
        FreezePlayer(); // Đóng băng player
        Debug.Log("[MealGameManager] 🎉 THẮNG! Đã hoàn thành tất cả 4 bếp.");

        // Phát sự kiện Observer → MealPuzzleStrategy lắng nghe
        if (OnAllStationsCompleted == null)
            Debug.LogError("[MealGameManager] Không có Observer nhận sự kiện hoàn thành; tiến trình sẽ không được lưu.", this);

        OnAllStationsCompleted?.Invoke();

        // Lượt chơi đã kết thúc thành công; chỉ giữ mealCompleted xuyên scene.
        if (GameManager.Instance != null)
            GameManager.Instance.ResetMotherMealAttempt();

        if (winPanel != null)
            winPanel.SetActive(true);
    }

    // ── Thua ───────────────────────────────────────────
    private void TriggerLose()
    {
        if (_gameEnded) return;
        _gameEnded = true;

        spawner?.StopSpawning();
        FreezePlayer(); // Đóng băng player
        Debug.Log("[MealGameManager] ⏰ THUA! Hết thời gian.");

        // Không mang tiến trình dở dang sang lần retry/lần vào bếp sau.
        if (GameManager.Instance != null)
            GameManager.Instance.ResetMotherMealAttempt();

        if (losePanel != null)
            losePanel.SetActive(true);
    }

    // ── Đóng băng nhân vật ───────────────────────────────
    private void FreezePlayer()
    {
        // Tìm PlayerController và tắt để ngừng di chuyển
        var player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.enabled = false;

            // Dừng hẳn velocity nếu có Rigidbody2D
            var rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;

            Debug.Log("[MealGameManager] Đã dừng di chuyển nhân vật.");
        }
    }

    // ── Nút UI ────────────────────────────────────────────────
    /// <summary>Gán vào nút "Thử lại" trên LosePanel.</summary>
    public void RetryChapter()
    {
        // Phòng trường hợp nút Retry được gọi từ một luồng khác ngoài
        // TriggerLose, reset thêm lần nữa vẫn an toàn và idempotent.
        if (GameManager.Instance != null)
            GameManager.Instance.ResetMotherMealAttempt();

        SceneTransitionLoader.LoadScene(SceneManager.GetActiveScene().name, this);
    }

    /// <summary>Gán vào nút "Quay về" trên WinPanel/LosePanel.</summary>
    public void ReturnToMomRoom()
    {
        SceneTransitionLoader.LoadScene(momRoomScene, this);
    }

    // ── Cập nhật UI đồng hồ ───────────────────────────────────
    private void UpdateTimerUI()
    {
        if (timerText == null) return;
        int minutes = Mathf.FloorToInt(_timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(_timeRemaining % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}
