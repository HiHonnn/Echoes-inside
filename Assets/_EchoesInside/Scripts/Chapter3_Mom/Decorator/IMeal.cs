/// <summary>
/// Component Interface của Decorator Pattern — Chapter 3.
/// Mọi lớp món ăn (BasicMeal và các IngredientDecorator) đều implement interface này.
/// </summary>
public interface IMeal
{
    /// <summary>Trả về mô tả tên món tích lũy qua các lớp Decorator.</summary>
    string GetDescription();

    /// <summary>Trả về tỷ lệ hoàn thành (0.0 → 1.0) tích lũy qua các lớp Decorator.</summary>
    float GetCompletionRate();
}
