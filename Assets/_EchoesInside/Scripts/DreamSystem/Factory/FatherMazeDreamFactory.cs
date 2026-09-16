/// <summary>
/// Abstract Factory cụ thể cho mini-game mê cung trong giấc mơ của Ba.
/// Tạo một họ sản phẩm đồng bộ: môi trường mê cung, puzzle thu thập,
/// enemy tuần tra và âm thanh mê cung.
/// </summary>
public class FatherMazeDreamFactory : IDreamFactory
{
    public IDreamEnvironment CreateEnvironment() => new MazeEnvironment();
    public IPuzzleStrategy CreatePuzzle()        => new MazePuzzleStrategy();
    public IDreamEnemy CreateEnemy()             => new MazeEnemySpawner();
    public IDreamAudio CreateMusic()              => new MazeMusic();
}
