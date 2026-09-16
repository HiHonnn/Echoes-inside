using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gắn lên Button "BtnComputer" trong Canvas.
/// Khi người dùng click vào vùng máy tính → load scene Chapter2_Maze.
/// </summary>
public class ComputerInteraction : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("Tên scene Chapter2_Maze cần load")]
    [SerializeField] private string mazeSceneName = "Chapter2_Maze";

    // ─────────────────────────────────────────────────────
    /// <summary>
    /// Gán hàm này vào Button.OnClick() trong Inspector.
    /// </summary>
    public void OnComputerClicked()
    {
        if (string.IsNullOrEmpty(mazeSceneName))
        {
            Debug.LogWarning("[ComputerInteraction] Chưa đặt tên scene mê cung!");
            return;
        }
        Debug.Log("[ComputerInteraction] Vào mê cung...");
        SceneTransitionLoader.LoadScene(mazeSceneName, this);
    }
}
