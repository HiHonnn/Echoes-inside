using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Trung tâm điều phối toàn bộ gameplay trong mê cung Chapter 2.
/// Singleton chỉ tồn tại trong Scene Chapter2_Maze.
/// </summary>
public class MazeGameManager : MonoBehaviour
{
    public static MazeGameManager Instance { get; private set; }

    public event Action OnMazeEscaped;

    // ── Cấu hình ──────────────────────────────────────────
    [Header("Cấu hình")]
    [Tooltip("Tổng số vật phẩm cần nhặt để mở cửa thoát")]
    [SerializeField] private int totalItems = 4;

    [Tooltip("Tên Scene phòng ba để quay về sau khi thoát mê cung")]
    [SerializeField] private string fatherSceneName = "Chapter2_Father";

    [Tooltip("Thời gian chờ sau khi bị bắt trước khi hồi sinh (giây)")]
    [SerializeField] private float respawnDelay = 1.5f;

    // ── UI References ──────────────────────────────────────
    [Header("UI")]
    [Tooltip("Text hiển thị số vật phẩm đã nhặt: '0 / 4'")]
    [SerializeField] private TMP_Text itemCounterText;

    [Tooltip("Panel màn hình thua (mờ đen + nút Thử lại)")]
    [SerializeField] private GameObject deathPanel;

    [Tooltip("Panel thông báo đầu mê cung")]
    [SerializeField] private GameObject introPanel;

    // ── Gameplay References ────────────────────────────────
    [Header("Gameplay")]
    [Tooltip("Transform điểm hồi sinh của Player")]
    [SerializeField] private Transform spawnPoint;

    [Tooltip("Script cửa thoát (MazeExitDoor)")]
    [SerializeField] private MazeExitDoor exitDoor;

    [Tooltip("GameObject của Player trong mê cung")]
    [SerializeField] private GameObject player;

    // ── Trạng thái nội bộ ─────────────────────────────────
    private int _collectedCount = 0;
    private bool _isGameOver = false;
    private bool _hasEscaped = false;
    private List<string> _collectedItemIds = new List<string>();
    private IDreamFactory _dreamFactory;
    private IDreamEnvironment _dreamEnvironment;
    private IDreamEnemy _dreamEnemy;
    private IDreamAudio _dreamMusic;
    private PuzzleController _puzzleController;

    private void OnValidate()
    {
        if (totalItems <= 0)
            Debug.LogWarning("[MazeGameManager] Total Items phải lớn hơn 0.", this);

        if (string.IsNullOrWhiteSpace(fatherSceneName))
            Debug.LogWarning("[MazeGameManager] Father Scene Name đang trống.", this);

        if (spawnPoint == null)
            Debug.LogWarning("[MazeGameManager] Chưa gán Spawn Point.", this);

        if (exitDoor == null)
            Debug.LogWarning("[MazeGameManager] Chưa gán Maze Exit Door.", this);

        if (player == null)
            Debug.LogWarning("[MazeGameManager] Chưa gán Player.", this);
    }

    // ─────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        _collectedCount = 0;
        _isGameOver = false;
        _hasEscaped = false;

        // Abstract Factory tạo trọn họ sản phẩm của mini-game mê cung.
        _dreamFactory = new FatherMazeDreamFactory();

        _dreamEnvironment = _dreamFactory.CreateEnvironment();
        _dreamEnvironment.Initialize();

        _dreamMusic = _dreamFactory.CreateMusic();
        _dreamMusic.Play();

        _dreamEnemy = _dreamFactory.CreateEnemy();
        _dreamEnemy.Spawn();

        _puzzleController = new PuzzleController();
        _puzzleController.SetStrategy(_dreamFactory.CreatePuzzle());
        _puzzleController.OnPuzzleCompleted += HandlePuzzleCompleted;
        _puzzleController.SetupPuzzle();
        _puzzleController.ExecutePuzzle();

        UpdateItemCounter();

        if (deathPanel != null) deathPanel.SetActive(false);

        // Hiện thông báo mở đầu mê cung
        if (introPanel != null)
        {
            introPanel.SetActive(true);
            StartCoroutine(HideIntroAfterDelay(3f));
        }

        // Khôi phục vật phẩm đã nhặt từ GameManager (nếu có)
        RestoreFromGameManager();
    }

    /// Ẩn panel intro sau vài giây
    private IEnumerator HideIntroAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (introPanel != null) introPanel.SetActive(false);
    }

    // ── Khôi phục tiến trình từ GameManager ──────────────
    private void RestoreFromGameManager()
    {
        if (GameManager.Instance == null) return;

        var state = GameManager.Instance.chapter2State;
        _collectedItemIds = new List<string>(state.collectedMazeItems);
        _collectedCount = _collectedItemIds.Count;
        UpdateItemCounter();

        // Nếu đã đủ vật phẩm từ trước → mở cửa thoát luôn
        if (_collectedCount >= totalItems && exitDoor != null)
            exitDoor.OpenDoor();
    }

    // ── Người chơi nhặt được vật phẩm ────────────────────
    public void OnItemCollected(string itemId)
    {
        if (_collectedItemIds.Contains(itemId)) return;

        _collectedItemIds.Add(itemId);
        _collectedCount++;

        // Lưu vào GameManager (Memento)
        if (GameManager.Instance != null)
            GameManager.Instance.chapter2State.collectedMazeItems.Add(itemId);

        UpdateItemCounter();
        Debug.Log($"[MazeGameManager] Đã nhặt: {itemId} ({_collectedCount}/{totalItems})");

        // Nếu đã đủ → mở cửa thoát
        if (_collectedCount >= totalItems)
            OpenExitDoor();
    }

    // ── Mở cửa thoát ─────────────────────────────────────
    private void OpenExitDoor()
    {
        Debug.Log("[MazeGameManager] Đã nhặt đủ vật phẩm! Cửa thoát mở ra.");
        if (exitDoor != null) exitDoor.OpenDoor();
    }

    // ── Người chơi bị bắt ────────────────────────────────
    public void OnPlayerCaught()
    {
        if (_isGameOver) return;
        Debug.Log("[MazeGameManager] Người chơi bị bắt! Hồi sinh...");
        StartCoroutine(RespawnSequence());
    }

    private IEnumerator RespawnSequence()
    {
        _isGameOver = true;

        // Hiện màn hình thua ngắn
        if (deathPanel != null) deathPanel.SetActive(true);

        yield return new WaitForSeconds(respawnDelay);

        // Tắt màn hình thua
        if (deathPanel != null) deathPanel.SetActive(false);

        // Dịch chuyển Player về SpawnPoint
        if (player != null && spawnPoint != null)
            player.transform.position = spawnPoint.position;

        _isGameOver = false;
    }

    // ── Người chơi thoát thành công ───────────────────────
    public void OnPlayerEscaped()
    {
        if (_isGameOver || _hasEscaped) return;
        _hasEscaped = true;
        Debug.Log("[MazeGameManager] Thoát mê cung thành công!");

        if (OnMazeEscaped == null)
            Debug.LogError("[MazeGameManager] Không có MazePuzzleStrategy nhận sự kiện thoát mê cung.", this);

        OnMazeEscaped?.Invoke();

        StartCoroutine(LoadFatherScene());
    }

    private void HandlePuzzleCompleted()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.CompleteFatherMaze();

        Debug.Log("[MazeGameManager] Strategy xác nhận puzzle mê cung hoàn thành, đã lưu tiến trình Chapter 2.");
    }

    private void OnDestroy()
    {
        if (_puzzleController != null)
        {
            _puzzleController.OnPuzzleCompleted -= HandlePuzzleCompleted;
            _puzzleController.Dispose();
        }

        _dreamMusic?.Stop();
    }

    private IEnumerator LoadFatherScene()
    {
        yield return new WaitForSeconds(1f);
        SceneTransitionLoader.LoadScene(fatherSceneName, this);
    }

    // ── Cập nhật UI bộ đếm vật phẩm ─────────────────────
    private void UpdateItemCounter()
    {
        if (itemCounterText != null)
            itemCounterText.text = $"{_collectedCount} / {totalItems}";
    }

    // ── Nút "Thử Lại" trên Death Panel ───────────────────
    public void RetryAfterDeath()
    {
        if (deathPanel != null) deathPanel.SetActive(false);
        if (player != null && spawnPoint != null)
            player.transform.position = spawnPoint.position;
        _isGameOver = false;
    }
}
