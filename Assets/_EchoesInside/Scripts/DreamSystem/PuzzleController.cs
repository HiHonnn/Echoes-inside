using System;

/// <summary>
/// Context của Strategy Pattern cho tất cả puzzle.
/// Context chỉ biết IPuzzleStrategy, chuyển tiếp sự kiện hoàn thành và
/// quản lý vòng đời của strategy đang được sử dụng.
/// </summary>
public sealed class PuzzleController : IDisposable
{
    private IPuzzleStrategy _strategy;

    public event Action OnPuzzleCompleted;

    public bool HasStrategy => _strategy != null;
    public bool IsCompleted => _strategy != null && _strategy.IsCompleted();

    public void SetStrategy(IPuzzleStrategy strategy)
    {
        if (ReferenceEquals(_strategy, strategy)) return;

        ReleaseCurrentStrategy();
        _strategy = strategy;

        if (_strategy != null)
            _strategy.OnPuzzleCompleted += HandlePuzzleCompleted;
    }

    public void SetupPuzzle()
    {
        _strategy?.Setup();
    }

    public void ExecutePuzzle()
    {
        _strategy?.ExecutePuzzle();
    }

    private void HandlePuzzleCompleted()
    {
        OnPuzzleCompleted?.Invoke();
    }

    private void ReleaseCurrentStrategy()
    {
        if (_strategy == null) return;

        _strategy.OnPuzzleCompleted -= HandlePuzzleCompleted;

        if (_strategy is IDisposable disposableStrategy)
            disposableStrategy.Dispose();

        _strategy = null;
    }

    public void Dispose()
    {
        ReleaseCurrentStrategy();
        OnPuzzleCompleted = null;
    }
}
