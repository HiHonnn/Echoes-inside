using System;

public interface IPuzzleStrategy
{
    void Setup();
    void ExecutePuzzle();
    bool IsCompleted();
    event Action OnPuzzleCompleted;
}
