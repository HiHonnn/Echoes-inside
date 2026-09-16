using System;
using UnityEngine;

// ── Product Classes cho phần Mê Cung trong Giấc Mơ Của Ba ────────────────
// Các class này là Concrete Products được tạo bởi FatherMazeDreamFactory.
//
// Luồng hoạt động:
//   FatherMazeDreamFactory.CreatePuzzle()  → MazePuzzleStrategy
//   FatherMazeDreamFactory.CreateEnemy()   → MazeEnemySpawner
//
// Khi player vào scene mê cung (Chapter2_Maze):
//   MazeEnvironment.Initialize()  → thiết lập môi trường mê cung
//   MazeEnemySpawner.Spawn()      → kích hoạt các EnemyPatrol[]
//   MazeMusic.Play()              → phát nhạc mê cung
// ──────────────────────────────────────────────────────────────────────────

// ── Product: Môi trường mê cung ──────────────────────────────
/// <summary>
/// Concrete Product — IDreamEnvironment.
/// Khởi tạo môi trường tối và sương mù cho mê cung trong giấc mơ của Ba.
/// </summary>
public class MazeEnvironment : IDreamEnvironment
{
    public void Initialize()
    {
        Debug.Log("[MazeEnvironment] Khởi tạo mê cung: Tắt ánh sáng, bật sương mù tâm lý.");
    }
}

// ── Product: Puzzle nhặt vật phẩm trong mê cung ──────────────
/// <summary>
/// Concrete Product — IPuzzleStrategy.
/// Câu đố của Chapter 2 (Ba): nhặt đủ vật phẩm trong mê cung để mở cửa thoát.
/// Được FatherMazeDreamFactory.CreatePuzzle() trả về.
/// </summary>
public class MazePuzzleStrategy : IPuzzleStrategy, IDisposable
{
    public event Action OnPuzzleCompleted;

    private MazeGameManager _mazeManager;
    private bool _isSubscribed;

    public void Setup()
    {
        if (_isSubscribed) return;

        _mazeManager = UnityEngine.Object.FindFirstObjectByType<MazeGameManager>();
        if (_mazeManager != null)
        {
            _mazeManager.OnMazeEscaped += HandleMazeEscaped;
            _isSubscribed = true;
            Debug.Log("[MazePuzzleStrategy] Đã kết nối với MazeGameManager. Sẵn sàng nhặt vật phẩm.");
        }
        else
        {
            // Bình thường khi gọi từ phòng ba — MazeGameManager chỉ có trong scene mê cung
            Debug.Log("[MazePuzzleStrategy] MazeGameManager chưa có (đang ở phòng ba). Sẽ kết nối khi vào mê cung.");
        }
    }

    public void ExecutePuzzle()
    {
        Debug.Log("[MazePuzzleStrategy] Câu đố đang chạy: Nhặt đủ vật phẩm để mở cửa thoát.");
    }

    private void HandleMazeEscaped()
    {
        Debug.Log("[MazePuzzleStrategy] Người chơi đã thoát mê cung → Puzzle hoàn thành.");
        OnPuzzleCompleted?.Invoke();
    }

    public void Dispose()
    {
        if (_isSubscribed && _mazeManager != null)
            _mazeManager.OnMazeEscaped -= HandleMazeEscaped;

        _mazeManager = null;
        _isSubscribed = false;
    }

    public bool IsCompleted()
    {
        return GameManager.Instance != null &&
               GameManager.Instance.chapter2State.mazeCompleted;
    }
}

// ── Product: Kẻ địch tuần tra trong mê cung ──────────────────
/// <summary>
/// Concrete Product — IDreamEnemy.
/// Kích hoạt tất cả EnemyPatrol[] trong scene mê cung.
/// </summary>
public class MazeEnemySpawner : IDreamEnemy
{
    public void Spawn()
    {
        EnemyPatrol[] enemies = UnityEngine.Object.FindObjectsByType<EnemyPatrol>(
            UnityEngine.FindObjectsSortMode.None);

        foreach (EnemyPatrol enemy in enemies)
        {
            enemy.enabled = true;
        }

        Debug.Log($"[MazeEnemySpawner] Đã kích hoạt {enemies.Length} kẻ địch tuần tra.");
    }
}

// ── Product: Nhạc nền mê cung ─────────────────────────────────
/// <summary>
/// Concrete Product — IDreamAudio.
/// Phát nhạc nền khi player đang trong mê cung.
/// </summary>
public class MazeMusic : IDreamAudio
{
    private SceneMusicPlayer _musicPlayer;

    public void Play()
    {
        _musicPlayer = UnityEngine.Object.FindFirstObjectByType<SceneMusicPlayer>();
        if (_musicPlayer != null)
        {
            _musicPlayer.Play();
            Debug.Log("[MazeMusic] Đang phát nhạc: Mê cung u ám.");
        }
        else
        {
            Debug.LogWarning("[MazeMusic] Không tìm thấy SceneMusicPlayer!");
        }
    }

    public void Stop()
    {
        if (_musicPlayer != null)
        {
            _musicPlayer.FadeOut(0f);
            Debug.Log("[MazeMusic] Dừng nhạc mê cung.");
        }
    }
}
