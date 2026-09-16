using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Quản lý túi đồ trong game.
/// Khi nhặt/xóa vật phẩm, đồng thời lưu trạng thái vào GameManager (Memento).
/// Khi scene được tải lại, khôi phục túi đồ từ GameManager.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("UI Slots")]
    [SerializeField] private List<InventorySlot> slots;

    [Header("Sprite map: dùng để khôi phục icon sau khi load scene")]
    [Tooltip("Kéo tất cả Sprite của vật phẩm vào đây đúng thứ tự ID")]
    [SerializeField] private List<ItemSpritePair> itemSpriteMap;

    [Header("Cấu hình Đóng/Mở túi đồ")]
    [SerializeField] private RectTransform inventoryPanel;
    [SerializeField] private Vector2 openPosition;
    [SerializeField] private Vector2 closedPosition;
    [SerializeField] private float slideSpeed = 10f;

    [Header("Nút Toggle (Mũi tên)")]
    [Tooltip("Kéo RectTransform của Btn_Toggle vào đây để lật mũi tên khi đóng/mở")]
    [SerializeField] private RectTransform toggleArrow;

    private bool _isOpen = false;
    private Vector2 _targetPosition;
    private string _selectedItemId = "";

    public string SelectedItemId => _selectedItemId;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _targetPosition = closedPosition;
            if (inventoryPanel != null)
                inventoryPanel.anchoredPosition = closedPosition;
            UpdateArrow();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnValidate()
    {
        if (slots == null || slots.Count == 0)
        {
            Debug.LogWarning("[InventoryManager] Chưa cấu hình Inventory Slot.", this);
        }
        else
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] == null)
                    Debug.LogWarning($"[InventoryManager] Slot tại index {i} chưa được gán.", this);
            }
        }

        if (itemSpriteMap == null) return;

        var knownIds = new HashSet<string>();
        foreach (var pair in itemSpriteMap)
        {
            if (pair == null || string.IsNullOrWhiteSpace(pair.itemId))
            {
                Debug.LogWarning("[InventoryManager] Item Sprite Map có phần tử thiếu Item ID.", this);
                continue;
            }

            if (!knownIds.Add(pair.itemId))
                Debug.LogWarning($"[InventoryManager] Item ID '{pair.itemId}' bị trùng trong Item Sprite Map.", this);

            if (pair.sprite == null)
                Debug.LogWarning($"[InventoryManager] Item '{pair.itemId}' chưa có Sprite.", this);
        }
    }

    private void Start()
    {
        // ── Memento: Khôi phục túi đồ từ GameManager khi Scene được tải lại ──
        RestoreFromGameManager();
    }

    /// <summary>
    /// Khôi phục túi đồ từ trạng thái đã lưu trong GameManager.
    /// Được gọi trong Start() mỗi khi Scene được tải.
    /// </summary>
    private void RestoreFromGameManager()
    {
        if (GameManager.Instance == null) return;

        var savedIds = GameManager.Instance.chapter1State.inventoryItemIds;
        if (savedIds == null || savedIds.Count == 0) return;

        foreach (string itemId in savedIds)
        {
            Sprite icon = GetSpriteForItem(itemId);
            // Gọi nội bộ để không trigger SaveItem lần nữa
            AddItemToSlot(itemId, icon);
        }

        Debug.Log($"[InventoryManager] Đã khôi phục {savedIds.Count} vật phẩm từ GameManager.");
    }

    /// Lấy Sprite tương ứng với ID vật phẩm từ map đã cấu hình trong Inspector
    private Sprite GetSpriteForItem(string itemId)
    {
        if (itemSpriteMap == null) return null;
        foreach (var pair in itemSpriteMap)
        {
            if (pair.itemId == itemId)
                return pair.sprite;
        }
        Debug.LogWarning($"[InventoryManager] Không tìm thấy sprite cho item '{itemId}'. Hãy thêm vào Item Sprite Map trong Inspector.");
        return null;
    }

    /// Thêm vật phẩm vào slot (không lưu vào GameManager — chỉ dùng nội bộ khi restore)
    private void AddItemToSlot(string itemId, Sprite icon)
    {
        foreach (var slot in slots)
        {
            if (!slot.isOccupied)
            {
                slot.SetItem(itemId, icon);
                return;
            }
        }
    }

    private void Update()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.anchoredPosition = Vector2.Lerp(
                inventoryPanel.anchoredPosition,
                _targetPosition,
                Time.deltaTime * slideSpeed
            );
        }
    }

    public void ToggleInventory()
    {
        _isOpen = !_isOpen;
        _targetPosition = _isOpen ? openPosition : closedPosition;
        UpdateArrow();
    }

    private void UpdateArrow()
    {
        if (toggleArrow == null) return;
        toggleArrow.localScale = new Vector3(_isOpen ? -1f : 1f, 1f, 1f);
    }

    /// <summary>
    /// Nhặt vật phẩm vào túi và lưu vào GameManager (Memento).
    /// </summary>
    public bool CollectItem(string itemId, Sprite itemIcon)
    {
        if (HasItem(itemId)) return false;

        foreach (var slot in slots)
        {
            if (!slot.isOccupied)
            {
                slot.SetItem(itemId, itemIcon);

                // ── Memento: Lưu vào GameManager ──
                if (GameManager.Instance != null)
                    GameManager.Instance.SaveItem(itemId);

                if (!_isOpen)
                {
                    _isOpen = true;
                    _targetPosition = openPosition;
                    UpdateArrow();
                }
                return true;
            }
        }
        Debug.LogWarning("Túi đồ đã đầy!");
        return false;
    }

    public bool HasItem(string itemId)
    {
        foreach (var slot in slots)
        {
            if (slot.isOccupied && slot.itemId == itemId)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Xóa vật phẩm khỏi túi và xóa khỏi GameManager (Memento).
    /// </summary>
    public void RemoveItem(string itemId)
    {
        if (_selectedItemId == itemId)
            DeselectItem();

        foreach (var slot in slots)
        {
            if (slot.isOccupied && slot.itemId == itemId)
            {
                slot.Clear();

                // ── Memento: Xóa khỏi GameManager ──
                if (GameManager.Instance != null)
                    GameManager.Instance.RemoveSavedItem(itemId);

                break;
            }
        }
    }

    public void SelectItem(string itemId)
    {
        _selectedItemId = itemId;
        foreach (var slot in slots)
            slot.SetHighlight(slot.isOccupied && slot.itemId == itemId);
        Debug.Log("Đang chọn vật phẩm: " + itemId);
    }

    public void DeselectItem()
    {
        _selectedItemId = "";
        foreach (var slot in slots)
            slot.SetHighlight(false);
        Debug.Log("Đã bỏ chọn vật phẩm");
    }
}

/// <summary>
/// Cấu trúc ánh xạ giữa ID vật phẩm và Sprite hiển thị trong túi đồ.
/// Dùng để khôi phục icon khi tải lại Scene.
/// </summary>
[System.Serializable]
public class ItemSpritePair
{
    public string itemId;
    public Sprite sprite;
}
