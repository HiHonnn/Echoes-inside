using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Một ô trong túi nguyên liệu Chapter 3.
/// Người chơi click vào ô để chọn nguyên liệu trước khi đặt vào bếp.
/// </summary>
public class KitchenSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image highlightBorder;

    public bool IsOccupied { get; private set; }
    public string IngredientId { get; private set; }

    // ── Thiết lập nguyên liệu vào ô ──────────────────────────
    public void SetIngredient(string id, Sprite icon)
    {
        IngredientId = id;
        IsOccupied = true;

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.color = Color.white;
            iconImage.preserveAspect = true; // Giữ tỉ lệ gốc, không bị méo
            iconImage.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"[KitchenSlot] iconImage chưa được gán trong Inspector! ({gameObject.name})");
        }

        SetHighlight(false);
    }

    // ── Xóa ô ────────────────────────────────────────────────
    public void Clear()
    {
        IngredientId = "";
        IsOccupied = false;

        if (iconImage != null)
        {
            iconImage.sprite = null;
            // Tắt hẳn GameObject để ô trống rõ ràng
            iconImage.gameObject.SetActive(false);
        }

        SetHighlight(false);
    }

    // ── Highlight (viền sáng khi chọn) ────────────────────────
    // ── Người chơi click vào ô ────────────────────────────────
    public void OnClick()
    {
        if (!IsOccupied) return;

        if (KitchenInventoryManager.Instance != null)
            KitchenInventoryManager.Instance.ToggleSelection(this);
    }

    public void SetHighlight(bool isOn)
    {
        if (highlightBorder == null)
        {
            if (isOn)
                Debug.LogWarning($"[KitchenSlot] Chua gan Highlight Border trong Inspector! ({gameObject.name})", this);

            return;
        }

        // Highlight Border thuong la mot GameObject con va co the dang bi tat
        // trong Hierarchy. Image.enabled = true se khong hien neu GameObject van inactive.
        if (isOn && !highlightBorder.gameObject.activeSelf)
            highlightBorder.gameObject.SetActive(true);

        highlightBorder.enabled = isOn;

        if (isOn)
        {
            // Dam bao vien khong bi trong suot. Khong doi sibling order vi UI
            // hien tai dung Icon de che phan giua cua Highlight, chi de lo vien.
            Color color = highlightBorder.color;
            if (color.a <= 0.01f)
            {
                color.a = 1f;
                highlightBorder.color = color;
            }

        }
    }
}
