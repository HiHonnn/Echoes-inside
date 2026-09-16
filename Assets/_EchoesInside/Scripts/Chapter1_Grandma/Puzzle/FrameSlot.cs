using UnityEngine;

public class FrameSlot : MonoBehaviour
{
    [Header("ID mảnh ghép yêu cầu (ví dụ: Piece_0)")]
    [SerializeField] private string requiredPieceId;

    [Header("Chỉ số lắp ráp tương ứng trong MemoryPuzzle (0 đến 3)")]
    [SerializeField] private int pieceIndex;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(requiredPieceId))
            Debug.LogWarning("[FrameSlot] Required Piece ID đang trống.", this);

        if (pieceIndex < 0 || pieceIndex > 3)
            Debug.LogWarning($"[FrameSlot] Piece Index {pieceIndex} nằm ngoài phạm vi 0-3.", this);
    }

    public void OnSlotClick()
    {
        if (InventoryManager.Instance != null && MemoryPuzzle.Instance != null)
        {
            // Kiểm tra xem người chơi có đang click chọn đúng mảnh ghép yêu cầu trong túi đồ hay không
            if (InventoryManager.Instance.SelectedItemId == requiredPieceId)
            {
                // Gọi MemoryPuzzle để kích hoạt hiện mảnh ghép trên khung tranh
                MemoryPuzzle.Instance.TryPlacePiece(pieceIndex);
                
                // Tiêu thụ vật phẩm: xóa khỏi túi đồ và bỏ chọn
                InventoryManager.Instance.RemoveItem(requiredPieceId);
            }
            else
            {
                Debug.Log($"Bạn phải chọn mảnh ghép '{requiredPieceId}' trong túi đồ trước khi lắp ráp!");
            }
        }
        else
        {
            Debug.LogError("Thiếu InventoryManager hoặc MemoryPuzzle trong Scene!");
        }
    }
}
