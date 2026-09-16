using UnityEngine;

/// <summary>
/// Các product hỗ trợ scene phòng Ba.
/// Abstract Factory của mini-game nằm ở FatherMazeDreamFactory.
/// </summary>
// ── Product: Môi trường phòng ba ────────────────────────────
public class FatherRoomEnvironment : IDreamEnvironment
{
    public void Initialize()
    {
        FatherRoomManager manager = UnityEngine.Object.FindFirstObjectByType<FatherRoomManager>();
        if (manager != null)
        {
            manager.RefreshRoomState();
            Debug.Log("[FatherRoomEnvironment] Đã khởi tạo phòng ba dựa theo trạng thái GameManager.");
        }
        else
        {
            Debug.LogWarning("[FatherRoomEnvironment] Không tìm thấy FatherRoomManager trong scene!");
        }
    }
}

// ── Product: Nhạc nền phòng ba ───────────────────────────────
public class FatherRoomMusic : IDreamAudio
{
    private SceneMusicPlayer _musicPlayer;

    public void Play()
    {
        _musicPlayer = UnityEngine.Object.FindFirstObjectByType<SceneMusicPlayer>();
        if (_musicPlayer != null)
        {
            _musicPlayer.Play();
            Debug.Log("[FatherRoomMusic] Đang phát nhạc nền: Phòng ba yên tĩnh.");
        }
        else
        {
            Debug.LogWarning("[FatherRoomMusic] Không tìm thấy SceneMusicPlayer!");
        }
    }

    public void Stop()
    {
        if (_musicPlayer != null)
        {
            _musicPlayer.FadeOut(0f);
            Debug.Log("[FatherRoomMusic] Dừng nhạc nền.");
        }
    }
}
