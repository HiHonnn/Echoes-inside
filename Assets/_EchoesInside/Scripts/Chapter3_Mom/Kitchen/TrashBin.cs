using UnityEngine;

/// <summary>
/// Thùng rác trong bếp Chapter 3.
/// Người chơi đứng gần và nhấn E → bỏ TẤT CẢ nguyên liệu trong túi.
/// Dùng khi nhặt lộn, cần làm trống túi để nhặt lại từ đầu.
/// </summary>
public class TrashBin : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject interactPrompt;

    private bool _playerNearby = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerNearby = true;
        if (interactPrompt != null) interactPrompt.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerNearby = false;
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    private void Update()
    {
        if (!_playerNearby) return;
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return))
            DiscardAll();
    }

    /// <summary>
    /// Bỏ TẤT CẢ nguyên liệu trong túi — người chơi phải nhặt lại từ đầu.
    /// </summary>
    private void DiscardAll()
    {
        var inv = KitchenInventoryManager.Instance;
        if (inv == null) return;

        int count = inv.RemoveAllIngredients();
        if (count > 0)
            Debug.Log($"[TrashBin] Đã bỏ {count} nguyên liệu vào thùng rác. Túi trống.");
        else
            Debug.Log("[TrashBin] Túi đã trống rồi.");
    }
}
