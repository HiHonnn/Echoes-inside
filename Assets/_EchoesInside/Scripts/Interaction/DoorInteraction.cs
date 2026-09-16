using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorInteraction : MonoBehaviour
{
    [Header("Cài đặt cửa")]
    [SerializeField] private string targetScene; // Tên Scene sẽ load
    [SerializeField] private bool isSelfDoor;  // Đây có phải cửa thứ 6 không?

    [Header("UI thông báo")]
    [SerializeField] private GameObject promptUI; // Text "Nhấn Enter để vào"

    private bool _playerNearby = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        _playerNearby = true;

        if (promptUI != null)
            promptUI.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        _playerNearby = false;

        if (promptUI != null)
            promptUI.SetActive(false);
    }

    private void Update()
    {
        if (!_playerNearby) return;

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.E))
        {
            TryEnterDoor();
        }
    }

    private void TryEnterDoor()
    {
        // Cửa thứ 6 cần điều kiện đặc biệt
        if (isSelfDoor && !GameManager.Instance.CanOpenSelfDoor)
        {
            Debug.Log("Cửa này chưa thể mở — Hãy hiểu các thành viên khác trước.");
            return;
        }

        if (string.IsNullOrEmpty(targetScene))
        {
            Debug.LogWarning("Chưa đặt tên Scene đích!");
            return;
        }

        // Lưu vị trí và Scene hiện tại của Player trước khi chuyển Scene
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && GameManager.Instance != null)
        {
            GameManager.Instance.lastMainScenePosition = player.transform.position;
            GameManager.Instance.savedSceneName = SceneManager.GetActiveScene().name;
            GameManager.Instance.hasSavedPosition = true;
            Debug.Log($"[DoorInteraction] Đã lưu vị trí Player: {player.transform.position} trong Scene: {GameManager.Instance.savedSceneName}");
        }

        ScreenFader screenFader = FindFirstObjectByType<ScreenFader>();
        if (screenFader != null)
        {
            screenFader.FadeOutAndLoadScene(targetScene);
            return;
        }

        SceneManager.LoadScene(targetScene);
    }
}
