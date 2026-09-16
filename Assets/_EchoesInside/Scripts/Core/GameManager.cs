using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lưu toàn bộ trạng thái của Chapter 1 theo Memento Pattern.
/// GameManager giữ object này xuyên scene (DontDestroyOnLoad).
/// </summary>
[System.Serializable]
public class Chapter1State
{
    // ── Túi đồ ────────────────────────────────────────────
    /// Danh sách ID vật phẩm đang có trong túi đồ khi rời scene
    public List<string> inventoryItemIds = new List<string>();

    // ── Mảnh ghép ảnh ─────────────────────────────────────
    /// Mảnh ghép nào đã được lắp vào khung (index 0-3)
    public bool[] placedPieces = new bool[4];

    /// Câu đố ghép ảnh đã hoàn thành chưa
    public bool puzzleCompleted = false;

    /// Đã nhặt khung tranh hoàn chỉnh chưa
    public bool framePickedUp = false;

    // ── Vật phẩm trong phòng (Hotspot) ────────────────────
    /// Gối đã được nhấc lên (lộ ra mảnh ảnh) chưa
    public bool pillowLifted = false;

    /// Danh sách các Hotspot vật phẩm đã được nhặt rồi (không cần hiện lại)
    public List<string> collectedHotspots = new List<string>();

    // ── Tiến trình hội thoại ─────────────────────────────
    /// Đã hoàn thành hội thoại với bà chưa
    public bool dialogueCompleted = false;
}

/// <summary>
/// Lưu trạng thái của Chapter 2 (Mê cung) theo Memento Pattern.
/// </summary>
[System.Serializable]
public class Chapter2State
{
    /// Đã thoát mê cung chưa
    public bool mazeCompleted = false;
    /// Đã xem hội thoại với ba chưa
    public bool dialogueCompleted = false;
    /// Danh sách ID vật phẩm đã nhặt trong mê cung
    public List<string> collectedMazeItems = new List<string>();
}

/// <summary>
/// Lưu trạng thái của Chapter 3 (Nấu ăn của Mẹ) theo Memento Pattern.
/// </summary>
[System.Serializable]
public class Chapter3State
{
    /// Đã hoàn thành tất cả 4 món ăn (thắng) chưa
    public bool mealCompleted = false;
    /// Đã xem hội thoại kết thúc với Mẹ chưa
    public bool dialogueCompleted = false;
    /// Danh sách ID gian bếp đã hoàn thành (grandma/father/sister/self)
    public List<string> completedStations = new List<string>();
}

public class GameManager : MonoBehaviour
{
    // ── Singleton ──────────────────────────────────────────
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = UnityEngine.Object.FindFirstObjectByType<GameManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameManager_AutoCreated");
                    _instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                    Debug.Log("[GameManager] Tự động tạo GameManager để tránh lỗi NullReference khi test trực tiếp scene.");
                }
            }
            return _instance;
        }
    }

    // ── Trạng thái chữa lành của từng thành viên ─────────
    public bool isGrandmaHealed = false;
    public bool isFatherHealed = false;
    public bool isMotherHealed = false;
    public bool isSisterHealed = false;
    public bool isBrotherHealed = false;

    /// Cửa thứ 6 chỉ mở khi TẤT CẢ đã được chữa lành
    public bool CanOpenSelfDoor =>
        isGrandmaHealed && isFatherHealed &&
        isMotherHealed && isSisterHealed &&
        isBrotherHealed;

    // ── Memento: Bản ghi trạng thái từng Chapter ─────────
    /// Trạng thái đã lưu của Chapter 1 (Bà nội)
    public Chapter1State chapter1State = new Chapter1State();
    /// Trạng thái đã lưu của Chapter 2 (Ba — Mê cung)
    public Chapter2State chapter2State = new Chapter2State();
    /// Trạng thái đã lưu của Chapter 3 (Mẹ — Nấu ăn)
    public Chapter3State chapter3State = new Chapter3State();

    [Header("Lưu vị trí Player giữa các Scene")]
    [HideInInspector] public Vector3 lastMainScenePosition;
    [HideInInspector] public string savedSceneName;
    [HideInInspector] public bool hasSavedPosition = false;

    // ── Vòng đời ──────────────────────────────────────────
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject); // Tồn tại xuyên Scene
    }

    // ─────────────────────────────────────────────────────
    // API tiến trình Chapter
    // Giữ việc thay đổi trạng thái ở một nơi để tránh các script
    // đánh dấu "được chữa lành" trước khi hội thoại kết thúc.
    // ─────────────────────────────────────────────────────

    public void CompleteGrandmaPuzzle()
    {
        chapter1State.puzzleCompleted = true;
    }

    public void HealGrandma()
    {
        chapter1State.dialogueCompleted = true;
        isGrandmaHealed = true;
    }

    public void CompleteFatherMaze()
    {
        chapter2State.mazeCompleted = true;
    }

    public void HealFather()
    {
        chapter2State.dialogueCompleted = true;
        isFatherHealed = true;
    }

    public void CompleteMotherMeal()
    {
        chapter3State.mealCompleted = true;
    }

    /// <summary>
    /// Bắt đầu một lượt chơi bếp mới. Tiến trình từng bếp chỉ thuộc
    /// lượt chơi hiện tại, không được mang sang lần retry tiếp theo.
    /// </summary>
    public void BeginMotherMealAttempt()
    {
        ResetMotherMealAttempt();
    }

    /// <summary>Lưu một station đã hoàn thành, không cho phép ID trùng.</summary>
    public void SaveMotherStationCompleted(string stationId)
    {
        if (string.IsNullOrWhiteSpace(stationId)) return;

        if (!chapter3State.completedStations.Contains(stationId))
            chapter3State.completedStations.Add(stationId);
    }

    /// <summary>Xóa tiến trình tạm của lượt chơi bếp hiện tại.</summary>
    public void ResetMotherMealAttempt()
    {
        chapter3State.completedStations.Clear();
    }

    public void HealMother()
    {
        chapter3State.dialogueCompleted = true;
        isMotherHealed = true;
    }

    // ─────────────────────────────────────────────────────
    // API tiện dụng để các script khác thao tác với túi đồ
    // ─────────────────────────────────────────────────────

    /// Kiểm tra vật phẩm có trong snapshot túi đồ không
    public bool HasSavedItem(string itemId)
    {
        return chapter1State.inventoryItemIds.Contains(itemId);
    }

    /// Thêm vật phẩm vào snapshot túi đồ
    public void SaveItem(string itemId)
    {
        if (!chapter1State.inventoryItemIds.Contains(itemId))
            chapter1State.inventoryItemIds.Add(itemId);
    }

    /// Xóa vật phẩm khỏi snapshot túi đồ
    public void RemoveSavedItem(string itemId)
    {
        chapter1State.inventoryItemIds.Remove(itemId);
    }

    /// Lưu trạng thái mảnh ghép
    public void SavePiecePlaced(int index)
    {
        if (index >= 0 && index < chapter1State.placedPieces.Length)
            chapter1State.placedPieces[index] = true;
    }

    /// Đánh dấu hotspot vật phẩm đã được nhặt
    public void SaveHotspotCollected(string hotspotId)
    {
        if (!chapter1State.collectedHotspots.Contains(hotspotId))
            chapter1State.collectedHotspots.Add(hotspotId);
    }

    /// Kiểm tra hotspot đã được nhặt chưa
    public bool IsHotspotCollected(string hotspotId)
    {
        return chapter1State.collectedHotspots.Contains(hotspotId);
    }
}
