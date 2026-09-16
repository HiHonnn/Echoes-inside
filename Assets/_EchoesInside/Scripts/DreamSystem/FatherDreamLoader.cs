using UnityEngine;

/// <summary>
/// Bộ khởi tạo nhẹ cho scene phòng Ba.
/// Scene này là phần dẫn truyện nên không dùng Abstract Factory;
/// mini-game mê cung sử dụng FatherMazeDreamFactory riêng.
/// </summary>
public class FatherDreamLoader : MonoBehaviour
{
    private IDreamEnvironment _environment;
    private IDreamAudio      _music;

    // ─────────────────────────────────────────────────────
    private void Start()
    {
        // Phòng Ba chỉ cần khôi phục trạng thái phòng và phát nhạc nền.
        _environment = new FatherRoomEnvironment();
        _environment.Initialize();

        _music = new FatherRoomMusic();
        _music.Play();

        Debug.Log("[FatherDreamLoader] Đã khởi tạo scene phòng Ba.");
    }

    // ─────────────────────────────────────────────────────
    private void OnDestroy()
    {
        _music?.Stop();
    }
}
