using UnityEngine;

/// <summary>
/// Nguyên liệu nấu ăn rơi từ đỉnh màn hình xuống.
/// Va chạm với Player → thêm vào KitchenInventoryManager.
/// Va chạm với Ground → tự hủy (người chơi không kịp nhặt).
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class IngredientItem : MonoBehaviour
{
    [Header("Thông tin nguyên liệu")]
    [Tooltip("ID khớp với RecipeIngredients trong CookingStation.\n" +
             "Ví dụ: rice / egg / veggie / meat / mushroom / spice / carrot / noodle / onion")]
    [SerializeField] public string ingredientId;

    [SerializeField] private Sprite ingredientSprite;

    private bool _collected = false;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(ingredientId))
            Debug.LogWarning("[IngredientItem] Ingredient ID đang trống.", this);

        if (ingredientSprite == null)
            Debug.LogWarning($"[IngredientItem] '{ingredientId}' chưa được gán Sprite.", this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_collected) return;

        if (other.CompareTag("Player"))
        {
            bool success = KitchenInventoryManager.Instance.AddIngredient(ingredientId, ingredientSprite);
            if (success)
            {
                _collected = true;
                Destroy(gameObject);
            }
            // Nếu túi đầy → nguyên liệu vẫn còn đó, người chơi tự xử lý
        }
        else if (other.CompareTag("Ground"))
        {
            // Rơi xuống sàn mà không ai nhặt → biến mất
            Destroy(gameObject);
        }
    }
}
