using UnityEngine;

public class GrandmaDreamLoader : MonoBehaviour
{
    private IDreamFactory _factory;
    private IDreamEnvironment _environment;
    private PuzzleController _puzzleController;
    private IDreamEnemy _enemy;
    private IDreamAudio _music;

    void Start()
    {
        // Khởi tạo Factory cho Giấc mơ của Bà (Chapter 1)
        _factory = new GrandmaDreamFactory();

        // 1. Khởi tạo Môi trường
        _environment = _factory.CreateEnvironment();
        _environment.Initialize();

        // 2. Khởi tạo Nhạc nền
        _music = _factory.CreateMusic();
        _music.Play();

        // 3. Khởi tạo Kẻ địch (NullEnemy)
        _enemy = _factory.CreateEnemy();
        _enemy.Spawn();

        // 4. Cài đặt Puzzle trong Scene
        _puzzleController = new PuzzleController();
        _puzzleController.SetStrategy(_factory.CreatePuzzle());
        if (_puzzleController.HasStrategy)
        {
            // Chỉ setup tại đây; UI Chapter 1 sẽ gọi ExecutePuzzle khi
            // người chơi thực sự mở khung ảnh.
            _puzzleController.SetupPuzzle();
            Debug.Log("Đã thiết lập Puzzle cho Chapter 1 thành công.");
        }
        else
        {
            Debug.LogWarning("Chưa tìm thấy MemoryPuzzle trong Scene (Sẽ cài đặt ở bước sau).");
        }
    }

    private void OnDestroy()
    {
        _puzzleController?.Dispose();

        if (_music != null)
        {
            _music.Stop();
        }
    }
}
