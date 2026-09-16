/// <summary>
/// Concrete Component của Decorator Pattern.
/// Đây là trạng thái ban đầu của bếp (nồi/chảo trống),
/// là đối tượng gốc được các IngredientDecorator bọc lên.
/// </summary>
public class BasicMeal : IMeal
{
    private readonly string _initialName;

    public BasicMeal(string initialName = "Nồi trống")
    {
        _initialName = initialName;
    }

    public string GetDescription() => _initialName;
    public float GetCompletionRate() => 0f;
}
