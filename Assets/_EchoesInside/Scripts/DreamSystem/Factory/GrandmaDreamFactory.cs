using UnityEngine;

public class GrandmaDreamFactory : IDreamFactory
{
    public IDreamEnvironment CreateEnvironment() => new SunsetFieldEnvironment();
    
    public IPuzzleStrategy CreatePuzzle()
    {
        // Tìm MemoryPuzzle đang chạy trong Scene
        return Object.FindFirstObjectByType<MemoryPuzzle>();
    }
    
    public IDreamEnemy CreateEnemy() => new NullEnemy();
    
    public IDreamAudio CreateMusic() => new OldRadioMusic();
}

public class SunsetFieldEnvironment : IDreamEnvironment
{
    public void Initialize()
    {
        Debug.Log("Khởi tạo môi trường Giấc mơ của Bà: Cánh đồng hoàng hôn.");
    }
}

public class NullEnemy : IDreamEnemy
{
    public void Spawn()
    {
        Debug.Log("Không có kẻ địch trong Giấc mơ của Bà (Sử dụng Null Object Pattern).");
    }
}

public class OldRadioMusic : IDreamAudio
{
    private SceneMusicPlayer _musicPlayer;

    public void Play()
    {
        _musicPlayer = Object.FindFirstObjectByType<SceneMusicPlayer>();
        if (_musicPlayer != null)
        {
            _musicPlayer.Play();
            Debug.Log("Đang phát nhạc: Radio cũ của Bà.");
        }
        else
        {
            Debug.LogWarning("Không tìm thấy SceneMusicPlayer trong scene để phát nhạc!");
        }
    }

    public void Stop()
    {
        if (_musicPlayer != null)
        {
            _musicPlayer.FadeOut(0f);
            Debug.Log("Dừng phát nhạc.");
        }
    }
}
