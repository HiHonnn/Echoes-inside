using System;
using UnityEngine;

/// <summary>
/// Quản lý câu đố ghép ảnh của Chapter 1.
/// Khi mỗi mảnh được lắp vào, lưu trạng thái vào GameManager (Memento).
/// Khi Scene tải lại, khôi phục các mảnh đã lắp từ GameManager.
/// </summary>
public class MemoryPuzzle : MonoBehaviour, IPuzzleStrategy
{
    public static MemoryPuzzle Instance { get; private set; }

    [Header("Cấu hình Puzzle")]
    [SerializeField] private int totalPieces = 4;

    [Header("UI Khung Tranh (Frame Slots)")]
    [Tooltip("4 ô mảnh ghép trong khung tranh, ban đầu ẩn, khi ghép vào sẽ hiện")]
    [SerializeField] private GameObject[] frameSlots;

    [Header("Cảnh chính & Cảnh cận cảnh")]
    [SerializeField] private GameObject panelFrameAssembly;
    [SerializeField] private GameObject mainRoomView;

    private bool[] _placedPieces;
    private bool _isAssembled = false;

    public event Action OnPuzzleCompleted;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Setup()
    {
        _placedPieces = new bool[totalPieces];
        _isAssembled = false;

        // Ẩn tất cả mảnh trong khung tranh lúc bắt đầu
        foreach (var slot in frameSlots)
        {
            if (slot != null) slot.SetActive(false);
        }

        // ── Memento: Khôi phục các mảnh đã lắp từ GameManager ──
        RestoreFromGameManager();

        Debug.Log("Memory Puzzle: Cài đặt màn chơi. Hãy đi tìm " + totalPieces + " mảnh ghép ảnh.");
    }

    /// <summary>
    /// Khôi phục các mảnh ảnh đã lắp trước đó từ trạng thái lưu trong GameManager.
    /// </summary>
    private void RestoreFromGameManager()
    {
        if (GameManager.Instance == null) return;

        var state = GameManager.Instance.chapter1State;

        // Khôi phục từng mảnh đã lắp
        for (int i = 0; i < totalPieces; i++)
        {
            if (state.placedPieces != null && i < state.placedPieces.Length && state.placedPieces[i])
            {
                _placedPieces[i] = true;
                if (frameSlots[i] != null)
                    frameSlots[i].SetActive(true);
            }
        }

        // Nếu câu đố đã hoàn thành trước đó
        if (state.puzzleCompleted)
        {
            _isAssembled = true;
            Debug.Log("[MemoryPuzzle] Khôi phục: Câu đố đã hoàn thành từ lần chơi trước.");
        }
    }

    public void ExecutePuzzle()
    {
        if (panelFrameAssembly != null)
            panelFrameAssembly.SetActive(true);
        if (mainRoomView != null)
            mainRoomView.SetActive(false);
    }

    public bool IsCompleted() => _isAssembled;

    /// <summary>
    /// Gọi khi click vào ô trong Khung Tranh để lắp mảnh ghép vào.
    /// Tự động lưu trạng thái vào GameManager sau khi lắp thành công.
    /// </summary>
    public void TryPlacePiece(int index)
    {
        if (index < 0 || index >= totalPieces) return;

        if (!_placedPieces[index])
        {
            _placedPieces[index] = true;

            if (frameSlots[index] != null)
                frameSlots[index].SetActive(true);

            // ── Memento: Lưu mảnh đã lắp vào GameManager ──
            if (GameManager.Instance != null)
                GameManager.Instance.SavePiecePlaced(index);

            Debug.Log($"Đã lắp mảnh ghép số {index} vào khung!");

            if (CheckAllPlaced())
                CompleteAssembly();
        }
    }

    private bool CheckAllPlaced()
    {
        foreach (bool placed in _placedPieces)
        {
            if (!placed) return false;
        }
        return true;
    }

    public void CompleteAssembly()
    {
        _isAssembled = true;
        Debug.Log("Bức ảnh đã được lắp ghép hoàn chỉnh vào khung! Hãy mang bức ảnh đến nói chuyện với Bà.");

        // ── Lưu trạng thái hoàn thành vào GameManager ──
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CompleteGrandmaPuzzle();
        }

        OnPuzzleCompleted?.Invoke();
    }
}
