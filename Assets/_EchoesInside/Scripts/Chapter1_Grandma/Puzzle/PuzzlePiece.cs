using UnityEngine;

/// <summary>
/// Gắn lên từng mảnh ảnh trong phòng (trong cận cảnh).
/// - Khi người chơi nhặt: ẩn vật phẩm và lưu vào GameManager (Memento).
/// - Khi Scene tải lại: nếu đã nhặt rồi thì tự động ẩn, không cho nhặt lại.
/// </summary>
public class PuzzlePiece : MonoBehaviour
{
    [Header("ID định danh mảnh ghép (ví dụ: Piece_0)")]
    [SerializeField] private string pieceId;

    [Header("Icon hiển thị trong túi đồ")]
    [SerializeField] private Sprite pieceIcon;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(pieceId))
            Debug.LogWarning("[PuzzlePiece] Piece ID đang trống.", this);

        if (pieceIcon == null)
            Debug.LogWarning($"[PuzzlePiece] '{pieceId}' chưa được gán icon.", this);
    }

    private void Start()
    {
        // ── Memento: Nếu mảnh này đã được nhặt rồi → tự ẩn đi ──
        // GameManager.HasSavedItem kiểm tra cả trong túi đồ (inventoryItemIds)
        // lẫn trường hợp đã dùng xong (collectedHotspots)
        if (GameManager.Instance != null &&
            (GameManager.Instance.HasSavedItem(pieceId) ||
             GameManager.Instance.IsHotspotCollected(pieceId)))
        {
            gameObject.SetActive(false);
        }
    }

    public void Collect()
    {
        if (InventoryManager.Instance == null || pieceIcon == null)
        {
            Debug.LogError("Thiếu InventoryManager trong Scene hoặc chưa gán PieceIcon!");
            return;
        }

        bool success = InventoryManager.Instance.CollectItem(pieceId, pieceIcon);
        if (success)
        {
            // ── Memento: Đánh dấu hotspot đã được nhặt vào GameManager ──
            // Lưu vào collectedHotspots để kể cả sau khi dùng (xóa khỏi túi)
            // thì mảnh này vẫn không xuất hiện lại trong phòng
            if (GameManager.Instance != null)
                GameManager.Instance.SaveHotspotCollected(pieceId);

            // Ẩn mảnh ghép khỏi phòng cận cảnh hiện tại
            gameObject.SetActive(false);
        }
    }
}
