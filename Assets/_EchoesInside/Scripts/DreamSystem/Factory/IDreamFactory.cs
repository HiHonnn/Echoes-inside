public interface IDreamFactory
{
    IDreamEnvironment CreateEnvironment();
    IPuzzleStrategy CreatePuzzle();
    IDreamEnemy CreateEnemy();
    IDreamAudio CreateMusic();
}

public interface IDreamEnvironment
{
    void Initialize();
}

public interface IDreamEnemy
{
    void Spawn();
}

public interface IDreamAudio
{
    void Play();
    void Stop();
}
