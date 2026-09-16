using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [Header("UI Component hiển thị ảnh vật phẩm")]
    [SerializeField] private Image iconImage;

    [Header("UI GameObject viền ngoài highlight")]
    [SerializeField] private GameObject outlineObject;

    public string itemId { get; private set; } = "";
    public bool isOccupied { get; private set; } = false;

    private void Awake()
    {
        if (iconImage != null)
        {
            iconImage.preserveAspect = true; // Tự động giữ nguyên tỉ lệ ảnh để tránh bóp méo
        }
        Clear();
    }

    public void SetItem(string id, Sprite icon)
    {
        itemId = id;
        isOccupied = true;
        
        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.preserveAspect = true; // Đảm bảo ảnh không bị bóp méo
            iconImage.gameObject.SetActive(true);
        }

        SetHighlight(false);
    }

    public void Clear()
    {
        itemId = "";
        isOccupied = false;
        
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.gameObject.SetActive(false);
        }

        SetHighlight(false);
    }

    // Sự kiện được gọi khi người chơi Click vào ô túi đồ này
    public void OnSlotClicked()
    {
        Debug.Log($"[Slot Click] Bạn vừa click vào: {gameObject.name}. isOccupied: {isOccupied}, itemId: '{itemId}', outlineObject: {(outlineObject != null ? "Đã gán" : "Chưa gán")}");
        
        if (!isOccupied) return;

        if (InventoryManager.Instance != null)
        {
            if (InventoryManager.Instance.SelectedItemId == itemId)
            {
                InventoryManager.Instance.DeselectItem();
            }
            else
            {
                InventoryManager.Instance.SelectItem(itemId);
            }
        }
    }

    public void SetHighlight(bool isSelected)
    {
        if (outlineObject != null)
        {
            outlineObject.SetActive(isSelected);
        }
    }
}
