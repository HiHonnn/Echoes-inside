using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToMainScene : MonoBehaviour
{
    [Header("Tên Scene chính để quay lại")]
    [SerializeField] private string mainSceneName = "SampleScene";

    /// <summary>
    /// Hàm dùng để gọi từ OnClick Event của Button
    /// </summary>
    public void ReturnToMain()
    {
        if (string.IsNullOrEmpty(mainSceneName))
        {
            Debug.LogWarning("Chưa cấu hình tên Scene chính!");
            return;
        }
        SceneTransitionLoader.LoadScene(mainSceneName, this);
    }
}
