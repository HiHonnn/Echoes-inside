using UnityEngine;

/// <summary>
/// Cửa thoát của mê cung.
/// Ban đầu hiện CloseDoor, ẩn OpenDoor.
/// Khi MazeGameManager gọi OpenDoor() → ẩn CloseDoor, hiện OpenDoor,
/// bật Collider trigger để Player bước vào thoát.
/// </summary>
public class MazeExitDoor : MonoBehaviour
{
    [Header("Hình ảnh cửa")]
    [Tooltip("GameObject con chứa ảnh CỬA ĐÓNG — hiện lúc đầu")]
    [SerializeField] private GameObject closeDoorObject;

    [Tooltip("GameObject con chứa ảnh CỬA MỞ — hiện khi nhặt đủ vật phẩm")]
    [SerializeField] private GameObject openDoorObject;

    [Header("Hiệu ứng (tùy chọn)")]
    [Tooltip("Particle / ánh sáng khi cửa mở (để trống nếu chưa có)")]
    [SerializeField] private GameObject doorGlowEffect;

    [Header("Collider")]
    [Tooltip("Box Collider 2D trigger — tắt lúc đầu, bật khi cửa mở")]
    [SerializeField] private Collider2D doorCollider;

    private bool _isOpen = false;

    // ─────────────────────────────────────────────────────
    private void Awake()
    {
        // Trạng thái ban đầu: cửa đóng
        SetDoorState(isOpen: false);
    }

    // ─────────────────────────────────────────────────────
    /// <summary>
    /// Gọi từ MazeGameManager khi Player nhặt đủ vật phẩm.
    /// </summary>
    public void OpenDoor()
    {
        if (_isOpen) return;
        _isOpen = true;

        SetDoorState(isOpen: true);

        // Bật Collider trigger để Player có thể bước vào
        if (doorCollider != null) doorCollider.enabled = true;

        Debug.Log("[MazeExitDoor] Cửa thoát đã mở!");
    }

    // ─────────────────────────────────────────────────────
    private void SetDoorState(bool isOpen)
    {
        // Hiện/ẩn ảnh cửa đóng & cửa mở
        if (closeDoorObject != null) closeDoorObject.SetActive(!isOpen);
        if (openDoorObject  != null) openDoorObject.SetActive(isOpen);

        // Hiệu ứng glow chỉ bật khi cửa mở
        if (doorGlowEffect  != null) doorGlowEffect.SetActive(isOpen);

        // Collider tắt lúc đầu, chỉ bật khi mở
        if (doorCollider    != null) doorCollider.enabled = isOpen;
    }

    // ─────────────────────────────────────────────────────
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isOpen) return;
        if (!other.CompareTag("Player")) return;

        Debug.Log("[MazeExitDoor] Player thoát mê cung!");
        if (MazeGameManager.Instance != null)
            MazeGameManager.Instance.OnPlayerEscaped();
    }
}
