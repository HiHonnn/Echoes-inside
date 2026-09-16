/// <summary>
/// Abstract Factory cụ thể cho mini-game bếp trong giấc mơ của Mẹ.
/// Tạo một họ sản phẩm đồng bộ: môi trường bếp, puzzle nấu ăn,
/// Null Enemy và âm thanh bếp.
/// </summary>
public class MotherKitchenDreamFactory : IDreamFactory
{
    public IDreamEnvironment CreateEnvironment() => new KitchenEnvironment();
    public IPuzzleStrategy CreatePuzzle()        => new MealPuzzleStrategy();
    public IDreamEnemy CreateEnemy()             => new MotherNullEnemy();
    public IDreamAudio CreateMusic()              => new KitchenMusic();
}
