using UnityEngine;

/// <summary>
/// Gắn lên mỗi vật phẩm gia đình trong mê cung.
/// Khi Player chạm vào → báo MazeGameManager → ẩn vật phẩm.
/// </summary>
public class MazeFamilyItem : MonoBehaviour
{
    [Header("Thông tin vật phẩm")]
    [Tooltip("ID duy nhất của vật phẩm: FamilyPhoto, Toy, Letter, Gift")]
    [SerializeField] private string itemId;

    [Tooltip("Tên hiển thị khi nhặt được")]
    [SerializeField] private string itemDisplayName;

    [Header("Hiệu ứng")]
    [Tooltip("Panel/Text thông báo tên vật phẩm khi nhặt (tùy chọn)")]
    [SerializeField] private GameObject pickupNoticePanel;

    private bool _collected = false;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(itemId))
            Debug.LogWarning("[MazeFamilyItem] Item ID đang trống.", this);

        if (string.IsNullOrWhiteSpace(itemDisplayName))
            Debug.LogWarning($"[MazeFamilyItem] '{itemId}' chưa có Display Name.", this);
    }

    private void Start()
    {
        // Nếu vật phẩm này đã được nhặt rồi (từ lần chơi trước) → ẩn đi
        if (GameManager.Instance != null &&
            GameManager.Instance.chapter2State.collectedMazeItems.Contains(itemId))
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_collected) return;
        if (!other.CompareTag("Player")) return;

        _collected = true;

        Debug.Log($"[MazeFamilyItem] Nhặt được: {itemDisplayName}");

        // Báo cho MazeGameManager
        if (MazeGameManager.Instance != null)
            MazeGameManager.Instance.OnItemCollected(itemId);

        // Ẩn vật phẩm
        gameObject.SetActive(false);
    }
}
