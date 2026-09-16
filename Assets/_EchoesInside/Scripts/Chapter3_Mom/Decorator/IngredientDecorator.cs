/// <summary>
/// Abstract Decorator của Decorator Pattern.
/// Bọc một đối tượng IMeal khác và ủy quyền các phương thức cho nó,
/// đồng thời cộng dồn thêm mô tả và tỷ lệ hoàn thành.
/// Mọi Concrete Decorator (EggDecorator, MeatDecorator...) đều kế thừa lớp này.
/// </summary>
public abstract class IngredientDecorator : IMeal
{
    /// <summary>Tham chiếu đến đối tượng IMeal đang được bọc (Wrapped Component).</summary>
    protected readonly IMeal _meal;

    /// <summary>Phần trăm hoàn thành mà nguyên liệu này đóng góp (0.0 → 1.0).</summary>
    protected readonly float _contributionRate;

    protected IngredientDecorator(IMeal meal, float contributionRate)
    {
        _meal = meal;
        _contributionRate = contributionRate;
    }

    /// <summary>
    /// Mỗi Concrete Decorator phải ghi đè để nối thêm tên nguyên liệu vào mô tả.
    /// </summary>
    public abstract string GetDescription();

    /// <summary>
    /// Tự động cộng dồn tỷ lệ hoàn thành từ lớp bên trong + đóng góp của lớp này.
    /// Không cần ghi đè ở Concrete Decorator.
    /// </summary>
    public float GetCompletionRate()
        => _meal.GetCompletionRate() + _contributionRate;
}
