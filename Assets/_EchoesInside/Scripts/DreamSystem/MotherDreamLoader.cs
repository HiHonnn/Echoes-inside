using UnityEngine;
/// <summary>
/// Client Loader cho mini-game bếp trong giấc mơ của Mẹ.
/// Chỉ khởi tạo Abstract Factory khi đang ở scene có MealGameManager.
/// </summary>
public class MotherDreamLoader : MonoBehaviour
{
    private IDreamFactory _factory;
    private IDreamEnvironment _environment;
    private PuzzleController _puzzleController;
    private IDreamEnemy _enemy;
    private IDreamAudio _music;
    void Start()
    {
        // Component cũ vẫn tồn tại trong MomRoom để không làm mất scene
        // reference, nhưng phòng Mẹ là scene dẫn truyện nên không dùng Factory.
        if (MealGameManager.Instance == null)
        {
            Debug.Log("[MotherDreamLoader] Scene phòng Mẹ không cần Dream Factory.");
            enabled = false;
            return;
        }

        // ── Khởi tạo Factory cho mini-game bếp ──
        _factory = new MotherKitchenDreamFactory();
        // 1. Khởi tạo Môi trường
        _environment = _factory.CreateEnvironment();
        _environment.Initialize();
        // 2. Khởi tạo Nhạc
        _music = _factory.CreateMusic();
        _music.Play();
        // 3. Khởi tạo Kẻ địch
        _enemy = _factory.CreateEnemy();
        _enemy.Spawn();
        // 4. Khởi tạo Câu đố
        _puzzleController = new PuzzleController();
        _puzzleController.SetStrategy(_factory.CreatePuzzle());
        if (_puzzleController.HasStrategy)
        {
            _puzzleController.OnPuzzleCompleted += HandlePuzzleCompleted;
            _puzzleController.SetupPuzzle();
            _puzzleController.ExecutePuzzle();
        }
        Debug.Log("[MotherDreamLoader] Đã tải thành công Chapter 3: Giấc mơ của Mẹ!");
    }

    private void HandlePuzzleCompleted()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.CompleteMotherMeal();

        Debug.Log("[MotherDreamLoader] Puzzle bếp hoàn thành, đã lưu tiến trình Chapter 3.");
    }

    void OnDestroy()
    {
        if (_puzzleController != null)
        {
            _puzzleController.OnPuzzleCompleted -= HandlePuzzleCompleted;
            _puzzleController.Dispose();
        }

        _music?.Stop();
    }
}
