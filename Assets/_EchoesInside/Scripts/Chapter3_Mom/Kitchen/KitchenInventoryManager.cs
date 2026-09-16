using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Singleton quản lý túi nguyên liệu trong Chapter 3.
/// Tối đa 4 ô. Người chơi click ô để chọn, sau đó đứng gần bếp nhấn E để đặt vào.
///
/// Tách riêng với InventoryManager của Chapter 1 vì Chapter 3 dùng
/// nguyên liệu rơi (không phải item thông thường từ Hotspot).
/// </summary>
public class KitchenInventoryManager : MonoBehaviour
{
    public static KitchenInventoryManager Instance { get; private set; }

    private KitchenSlot _selectedSlot;

    public string SelectedIngredientId =>
        _selectedSlot != null && _selectedSlot.IsOccupied
            ? _selectedSlot.IngredientId
            : string.Empty;

    [Header("Slots UI (4 ô)")]
    [Tooltip("Kéo 4 KitchenSlot GameObject vào đây theo thứ tự")]
    [SerializeField] private List<KitchenSlot> slots;

    [Header("Thông báo")]
    [SerializeField] private GameObject fullWarningPanel;
    [Tooltip("Text hiện thông báo còn thiếu nguyên liệu gì")]
    [SerializeField] private TMP_Text missingMessageText;
    [SerializeField] private GameObject missingMessagePanel;

    // ─────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnValidate()
    {
        if (slots == null || slots.Count != 4)
        {
            Debug.LogWarning("[KitchenInventory] Cần cấu hình đúng 4 Kitchen Slot.", this);
        }
        else
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i] == null)
                    Debug.LogWarning($"[KitchenInventory] Slot tại index {i} chưa được gán.", this);
            }
        }

        if (missingMessagePanel == null)
            Debug.LogWarning("[KitchenInventory] Chưa gán Missing Message Panel.", this);

        if (missingMessageText == null)
            Debug.LogWarning("[KitchenInventory] Chưa gán Missing Message Text.", this);
    }

    /// <summary>
    /// Chọn duy nhất một slot nguyên liệu. Click lại slot đang chọn để bỏ chọn.
    /// </summary>
    public void ToggleSelection(KitchenSlot slot)
    {
        if (slot == null || slots == null || !slots.Contains(slot) || !slot.IsOccupied)
            return;

        if (_selectedSlot == slot)
        {
            ClearSelection();
            return;
        }

        ClearSelection();
        _selectedSlot = slot;
        _selectedSlot.SetHighlight(true);

        Debug.Log($"[KitchenInventory] Đã chọn '{_selectedSlot.IngredientId}'.");
    }

    public void ClearSelection()
    {
        if (_selectedSlot != null)
            _selectedSlot.SetHighlight(false);

        _selectedSlot = null;
    }

    /// <summary>
    /// Tiêu thụ đúng slot đang được chọn sau khi bếp chấp nhận nguyên liệu.
    /// </summary>
    public bool TryConsumeSelectedIngredient(out string ingredientId)
    {
        ingredientId = SelectedIngredientId;
        if (_selectedSlot == null || !_selectedSlot.IsOccupied || string.IsNullOrEmpty(ingredientId))
            return false;

        KitchenSlot consumedSlot = _selectedSlot;
        _selectedSlot = null;
        consumedSlot.Clear();

        Debug.Log($"[KitchenInventory] Đã dùng nguyên liệu đang chọn: '{ingredientId}'.");
        return true;
    }

    // ── Thêm nguyên liệu vào túi ─────────────────────────────
    /// <returns>true nếu thêm thành công, false nếu túi đầy</returns>
    public bool AddIngredient(string id, Sprite icon)
    {
        foreach (var slot in slots)
        {
            if (!slot.IsOccupied)
            {
                slot.SetIngredient(id, icon);
                Debug.Log($"[KitchenInventory] Nhặt '{id}' vào túi.");
                return true;
            }
        }

        // Túi đầy
        Debug.Log("[KitchenInventory] Túi đầy! Hãy nấu hoặc bỏ nguyên liệu trước.");
        if (fullWarningPanel != null)
            StartCoroutine(ShowWarningBriefly());
        return false;
    }

    // ── Kiểm tra túi có nguyên liệu này không ───────────────
    public bool HasIngredient(string id)
    {
        foreach (var slot in slots)
            if (slot.IsOccupied && slot.IngredientId == id)
                return true;
        return false;
    }

    // ── Xóa nguyên liệu khỏi túi ─────────────────────────────
    public void RemoveIngredient(string id)
    {
        foreach (var slot in slots)
        {
            if (slot.IsOccupied && slot.IngredientId == id)
            {
                if (_selectedSlot == slot)
                    _selectedSlot = null;

                slot.Clear();
                Debug.Log($"[KitchenInventory] Đã dùng '{id}' để nấu.");
                return;
            }
        }
    }

    // ── Hiện thông báo thiếu nguyên liệu ─────────────────────
    public void ShowMissingMessage(string missingList)
    {
        ShowMessage($"Còn thiếu: {missingList}");
    }

    public void ShowMessage(string message)
    {
        Debug.Log($"[KitchenInventory] {message}");
        if (missingMessagePanel != null && missingMessageText != null)
        {
            missingMessageText.text = message;
            StartCoroutine(ShowMessageBriefly(missingMessagePanel));
        }
    }

    private IEnumerator ShowMessageBriefly(GameObject panel)
    {
        panel.SetActive(true);
        yield return new WaitForSeconds(2f);
        panel.SetActive(false);
    }

    // ── Xóa TẤT CẢ nguyên liệu trong túi (dùng cho TrashBin) ──
    /// <returns>Số nguyên liệu đã xóa</returns>
    public int RemoveAllIngredients()
    {
        ClearSelection();

        int count = 0;
        foreach (var slot in slots)
        {
            if (slot.IsOccupied)
            {
                slot.Clear();
                count++;
            }
        }
        return count;
    }

    // ── Kiểm tra túi đầy ─────────────────────────────────────
    public bool IsFull()
    {
        foreach (var slot in slots)
            if (!slot.IsOccupied) return false;
        return true;
    }

    // ── Hiện cảnh báo túi đầy rồi tự ẩn ─────────────────────
    private IEnumerator ShowWarningBriefly()
    {
        if (fullWarningPanel != null) fullWarningPanel.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        if (fullWarningPanel != null) fullWarningPanel.SetActive(false);
    }
}
