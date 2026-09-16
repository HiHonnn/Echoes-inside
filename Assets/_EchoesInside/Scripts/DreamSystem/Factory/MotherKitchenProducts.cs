using System;
using UnityEngine;

// Các Concrete Product được MotherKitchenDreamFactory tạo cho scene bếp.

// ── Product: Môi trường bếp ────────────────────────────────────────
public class KitchenEnvironment : IDreamEnvironment
{
    public void Initialize()
    {
        MealGameManager manager = UnityEngine.Object.FindFirstObjectByType<MealGameManager>();
        if (manager != null)
        {
            Debug.Log("[KitchenEnvironment] Đã kết nối môi trường bếp với MealGameManager.");
        }
        else
        {
            Debug.LogWarning("[KitchenEnvironment] Không tìm thấy MealGameManager trong scene bếp!");
        }
    }
}

// ── Product: Nhạc bếp ký ức ────────────────────────────────────────
public class KitchenMusic : IDreamAudio
{
    private SceneMusicPlayer _musicPlayer;

    public void Play()
    {
        _musicPlayer = UnityEngine.Object.FindFirstObjectByType<SceneMusicPlayer>();
        if (_musicPlayer != null)
        {
            _musicPlayer.Play();
            Debug.Log("[KitchenMusic] Phát nhạc cho giấc mơ bếp.");
        }
        else
        {
            Debug.LogWarning("[KitchenMusic] Không tìm thấy SceneMusicPlayer!");
        }
    }

    public void Stop()
    {
        if (_musicPlayer != null)
        {
            _musicPlayer.FadeOut(0f);
            Debug.Log("[KitchenMusic] Dừng nhạc bếp.");
        }
    }
}

// ── Product: Không kẻ địch (Null Object Pattern) ─────────────────────
public class MotherNullEnemy : IDreamEnemy
{
    public void Spawn()
    {
        Debug.Log("[Mother Dream] Không có kẻ địch trong Chapter 3 (Null Object Pattern).");
    }
}

// ── Product: Câu đố nấu ăn (Strategy + kết hợp Observer) ─────────────
/// <summary>
/// Concrete Strategy cho câu đố Chapter 3.
/// Kết hợp Strategy Pattern (từ IDreamFactory) với Observer Pattern
/// (lắng nghe MealGameManager.OnAllStationsCompleted).
/// </summary>
public class MealPuzzleStrategy : IPuzzleStrategy, IDisposable
{
    // ── Observer: Phát sự kiện khi câu đố hoàn thành ──────────
    public event Action OnPuzzleCompleted;

    private MealGameManager _observedManager;
    private bool _isSubscribed;

    public void Setup()
    {
        Debug.Log("[Mother Dream] Thiết lập câu đố nấu ăn (Decorator Pattern) — 4 bếp, 3 phút.");
    }

    public void ExecutePuzzle()
    {
        Debug.Log("[Mother Dream] Bắt đầu gameplay bếp nấu ăn.");

        if (_isSubscribed) return;

        // Đăng ký lắng nghe MealGameManager (Observer Pattern)
        if (MealGameManager.Instance != null)
        {
            _observedManager = MealGameManager.Instance;
            _observedManager.OnAllStationsCompleted += HandleAllStationsCompleted;
            _isSubscribed = true;
            Debug.Log("[MealPuzzleStrategy] Đã đăng ký lắng nghe OnAllStationsCompleted.");
        }
        else
        {
            Debug.LogWarning("[MealPuzzleStrategy] Không tìm thấy MealGameManager trong Scene!");
        }
    }

    private void HandleAllStationsCompleted()
    {
        Debug.Log("[MealPuzzleStrategy] Tất cả 4 bếp hoàn thành → Kích hoạt OnPuzzleCompleted.");
        OnPuzzleCompleted?.Invoke();
    }

    public void Dispose()
    {
        if (_isSubscribed && _observedManager != null)
            _observedManager.OnAllStationsCompleted -= HandleAllStationsCompleted;

        _observedManager = null;
        _isSubscribed = false;
    }

    public bool IsCompleted()
    {
        return GameManager.Instance != null &&
               GameManager.Instance.chapter3State.mealCompleted;
    }
}
