using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Quản lý một gian bếp trong Chapter 3 — "Bữa Cơm Của Mẹ".
///
/// Cơ chế gameplay:
///   1. Đến gần bếp → RecipePanel hiện 4 icon nguyên liệu cần nhặt.
///   2. Click chọn một nguyên liệu trong túi → nhấn E để thêm vào bếp.
///   3. Mỗi nguyên liệu hợp lệ bọc thêm một Decorator và tăng tiến độ.
///   4. Đủ công thức → hiện tên món + ảnh món vừa nấu.
///
/// Decorator Pattern:
///   BasicMeal → Decorator1 → Decorator2 → Decorator3 → Decorator4
/// </summary>
public class CookingStation : MonoBehaviour
{
    // ── Cấu hình bếp ──────────────────────────────────────────
    [Header("Cấu hình bếp")]
    [Tooltip("ID duy nhất. Ví dụ: grandma / father / sister / self")]
    [SerializeField] public string stationId;

    [Tooltip("Tên ban đầu khi bếp trống")]
    [SerializeField] private string initialMealName = "Nồi trống";

    // ── Công thức ─────────────────────────────────────────────
    [Header("Công thức (Recipe)")]
    [Tooltip("Danh sách ID nguyên liệu cần nấu.\nVí dụ: rice, veggie, mushroom, spice")]
    [SerializeField] private List<string> recipeIngredients;

    [Tooltip("Sprite icon tương ứng với từng nguyên liệu (cùng thứ tự)")]
    [SerializeField] private List<Sprite> recipeSprites;

    // ── Recipe Panel (hiện khi player đến gần) ────────────────
    [Header("Recipe Panel — Hiện khi đến gần")]
    [SerializeField] private GameObject recipePanel;
    [Tooltip("Container chứa Title + SlotsRow bên trong Canvas_Station.\nKhi nấu xong, toàn bộ cụm này sẽ bị ẩn.")]
    [SerializeField] private GameObject recipeLayout;
    [SerializeField] private List<Image> recipeSlotImages;
    [SerializeField] private List<TMP_Text> recipeSlotLabels;

    // ── Completed Panel (hiện sau khi nấu xong) ───────────────
    [Header("Completed Info — Hiện sau khi nấu xong")]
    [Tooltip("Panel con bên trong Canvas_Station, hiện tên & ảnh món ăn")]
    [SerializeField] private GameObject completedInfoPanel;

    [Tooltip("Image hiển thị ảnh món ăn đã nấu")]
    [SerializeField] private Image completedDishImage;

    [Tooltip("Text hiển thị tên món ăn đã nấu")]
    [SerializeField] private TMP_Text completedDishText;

    [Tooltip("Sprite ảnh món ăn đã nấu (kéo ảnh món ăn vào đây)")]
    [SerializeField] private Sprite completedDishSprite;

    [Tooltip("Tên món ăn sau khi nấu xong. Ví dụ: Cháo của Bà")]
    [SerializeField] private string completedDishName = "Món ăn hoàn thành";

    // ── UI trạng thái bếp ─────────────────────────────────────
    [Header("UI trạng thái bếp")]
    [SerializeField] private TMP_Text mealDescriptionText;
    [SerializeField] private Slider progressBar;
    [SerializeField] private GameObject completedEffect;
    [SerializeField] private GameObject interactPrompt;

    // ── Sprite ────────────────────────────────────────────────
    [Header("Sprite")]
    [SerializeField] private SpriteRenderer stationRenderer;
    [SerializeField] private Sprite completedSprite;

    // ── Trạng thái ────────────────────────────────────────────
    private IMeal _currentMeal;
    private readonly HashSet<string> _addedIngredients = new HashSet<string>();
    private bool _isCompleted = false;
    private bool _playerNearby = false;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(stationId))
            Debug.LogWarning("[CookingStation] Station ID đang trống.", this);

        if (recipeIngredients == null || recipeIngredients.Count == 0)
        {
            Debug.LogWarning($"[CookingStation] '{stationId}' chưa có công thức.", this);
            return;
        }

        var uniqueIngredients = new HashSet<string>();
        foreach (string ingredient in recipeIngredients)
        {
            if (string.IsNullOrWhiteSpace(ingredient))
            {
                Debug.LogWarning($"[CookingStation] '{stationId}' có Ingredient ID trống.", this);
                continue;
            }

            if (!uniqueIngredients.Add(ingredient))
                Debug.LogWarning($"[CookingStation] '{stationId}' có nguyên liệu '{ingredient}' bị trùng.", this);

            if (!IngredientDecoratorFactory.IsSupported(ingredient))
                Debug.LogWarning($"[CookingStation] '{stationId}' dùng nguyên liệu không được hỗ trợ: '{ingredient}'.", this);
        }

        if (recipeSprites == null || recipeSprites.Count != recipeIngredients.Count)
            Debug.LogWarning($"[CookingStation] '{stationId}' cần số Recipe Sprite bằng số nguyên liệu.", this);

        if (recipeSlotImages == null || recipeSlotImages.Count != recipeIngredients.Count)
            Debug.LogWarning($"[CookingStation] '{stationId}' cần số Recipe Slot Image bằng số nguyên liệu.", this);

        if (completedInfoPanel != null && completedDishText == null)
            Debug.LogWarning($"[CookingStation] '{stationId}' chưa gán Completed Dish Text.", this);

        if (completedInfoPanel != null && completedDishImage == null)
            Debug.LogWarning($"[CookingStation] '{stationId}' chưa gán Completed Dish Image.", this);
    }

    // ─────────────────────────────────────────────────────────
    private void Start()
    {
        _currentMeal = new BasicMeal(initialMealName);
        _addedIngredients.Clear();

        InitRecipePanel();

        if (recipePanel != null)        recipePanel.SetActive(false);
        if (completedInfoPanel != null) completedInfoPanel.SetActive(false);

        UpdateUI();
    }

    // ── Khởi tạo RecipePanel ─────────────────────────────────
    private void InitRecipePanel()
    {
        for (int i = 0; i < recipeSlotImages.Count; i++)
        {
            if (recipeSlotImages[i] == null) continue;

            recipeSlotImages[i].color = Color.white;
            recipeSlotImages[i].preserveAspect = true;

            if (i < recipeSprites.Count && recipeSprites[i] != null)
            {
                recipeSlotImages[i].sprite = recipeSprites[i];
                recipeSlotImages[i].enabled = true;
            }

            if (recipeSlotLabels != null && i < recipeSlotLabels.Count && recipeSlotLabels[i] != null)
            {
                string label = (i < recipeIngredients.Count) ? recipeIngredients[i] : "";
                recipeSlotLabels[i].text = TranslateId(label);
                recipeSlotLabels[i].color = Color.white;
            }
        }
    }

    // ── Player đến gần ────────────────────────────────────────
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerNearby = true;

        if (_isCompleted)
        {
            // Bếp đã xong → hiện lại completed info khi player đến gần
            if (recipePanel != null) recipePanel.SetActive(true);
        }
        else
        {
            if (recipePanel != null)    recipePanel.SetActive(true);
            if (interactPrompt != null) interactPrompt.SetActive(true);
        }
    }

    // ── Player rời đi ─────────────────────────────────────────
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerNearby = false;

        if (recipePanel != null)    recipePanel.SetActive(false);
        if (interactPrompt != null) interactPrompt.SetActive(false);
    }

    private void Update()
    {
        if (!_playerNearby || _isCompleted) return;
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return))
            TryAddSelectedIngredient();
    }

    // ── Kiểm tra và nấu ──────────────────────────────────────
    private void TryAddSelectedIngredient()
    {
        var inv = KitchenInventoryManager.Instance;
        if (inv == null) return;

        if (!IsRecipeValidAtRuntime()) return;

        string selectedIngredient = inv.SelectedIngredientId;
        if (string.IsNullOrEmpty(selectedIngredient))
        {
            inv.ShowMessage("Hãy click chọn một nguyên liệu trong túi trước.");
            return;
        }

        if (!recipeIngredients.Contains(selectedIngredient))
        {
            inv.ShowMessage($"{TranslateId(selectedIngredient)} không thuộc công thức của bếp này.");
            return;
        }

        if (_addedIngredients.Contains(selectedIngredient))
        {
            inv.ShowMessage($"Bạn đã thêm {TranslateId(selectedIngredient)} vào món này rồi.");
            return;
        }

        float rate = 1f / recipeIngredients.Count;
        if (!IngredientDecoratorFactory.TryCreate(
                selectedIngredient,
                _currentMeal,
                rate,
                out IMeal decoratedMeal))
        {
            Debug.LogError($"[CookingStation] Không thể tạo Decorator cho '{selectedIngredient}'.", this);
            return;
        }

        // Chỉ cập nhật món sau khi inventory xác nhận đã tiêu thụ đúng slot.
        if (!inv.TryConsumeSelectedIngredient(out string consumedIngredient) ||
            consumedIngredient != selectedIngredient)
        {
            Debug.LogError("[CookingStation] Không thể tiêu thụ nguyên liệu đang chọn.", this);
            return;
        }

        _currentMeal = decoratedMeal;
        _addedIngredients.Add(selectedIngredient);
        MarkIngredientAsAdded(selectedIngredient);

        UpdateUI();
        Debug.Log(
            $"[CookingStation] '{stationId}' + {TranslateId(selectedIngredient)} " +
            $"({_addedIngredients.Count}/{recipeIngredients.Count}): {_currentMeal.GetDescription()}");

        if (_addedIngredients.Count >= recipeIngredients.Count)
            CompleteStation();
    }

    private void MarkIngredientAsAdded(string ingredientId)
    {
        int index = recipeIngredients.IndexOf(ingredientId);
        if (index < 0) return;

        if (recipeSlotImages != null &&
            index < recipeSlotImages.Count &&
            recipeSlotImages[index] != null)
        {
            Color iconColor = recipeSlotImages[index].color;
            iconColor.a = 0.3f;
            recipeSlotImages[index].color = iconColor;
        }

        if (recipeSlotLabels != null &&
            index < recipeSlotLabels.Count &&
            recipeSlotLabels[index] != null)
        {
            Color labelColor = recipeSlotLabels[index].color;
            labelColor.a = 0.3f;
            recipeSlotLabels[index].color = labelColor;
        }
    }

    private bool IsRecipeValidAtRuntime()
    {
        if (recipeIngredients == null || recipeIngredients.Count == 0)
        {
            Debug.LogError($"[CookingStation] '{stationId}' không có công thức hợp lệ.", this);
            return false;
        }

        var uniqueIngredients = new HashSet<string>();
        foreach (string ingredient in recipeIngredients)
        {
            if (!IngredientDecoratorFactory.IsSupported(ingredient))
            {
                Debug.LogError($"[CookingStation] Ingredient không được hỗ trợ: '{ingredient}'.", this);
                return false;
            }

            if (!uniqueIngredients.Add(ingredient))
            {
                Debug.LogError($"[CookingStation] Ingredient bị trùng trong recipe: '{ingredient}'.", this);
                return false;
            }
        }

        return true;
    }

    // ── Hoàn thành bếp ───────────────────────────────────────
    private void CompleteStation()
    {
        _isCompleted = true;

        // Ẩn prompt tương tác
        if (interactPrompt != null) interactPrompt.SetActive(false);

        // Đổi recipePanel thành hiện CompletedInfo
        ShowCompletedInfo();

        if (stationRenderer != null && completedSprite != null)
            stationRenderer.sprite = completedSprite;
        if (completedEffect != null)
            completedEffect.SetActive(true);

        MealGameManager.Instance?.OnStationCompleted(stationId);
    }

    /// <summary>
    /// Đổi nội dung Canvas_Station thành hiển thị món ăn đã nấu.
    /// Ẩn toàn bộ recipe layout, hiện CompletedInfoPanel.
    /// </summary>
    private void ShowCompletedInfo()
    {
        // Ẩn cả cụm recipe (Title + SlotsRow) bằng 1 lệnh
        if (recipeLayout != null)
            recipeLayout.SetActive(false);
        else
        {
            // Fallback: ẩn từng slot nếu không có recipeLayout
            foreach (var img in recipeSlotImages)
                if (img != null) img.gameObject.SetActive(false);
            if (recipeSlotLabels != null)
                foreach (var lbl in recipeSlotLabels)
                    if (lbl != null) lbl.gameObject.SetActive(false);
        }

        // Hiện completed info panel (tên + ảnh món)
        if (completedInfoPanel != null)
        {
            completedInfoPanel.SetActive(true);

            if (completedDishImage != null && completedDishSprite != null)
            {
                completedDishImage.sprite = completedDishSprite;
                completedDishImage.preserveAspect = true;
                completedDishImage.color = Color.white;
            }

            if (completedDishText != null)
                completedDishText.text = completedDishName;
        }

        // Giữ recipePanel bật để player đến gần vẫn thấy completedInfoPanel
        if (recipePanel != null) recipePanel.SetActive(true);
    }

    private void UpdateUI()
    {
        if (mealDescriptionText != null)
            mealDescriptionText.text = _currentMeal.GetDescription();
        if (progressBar != null)
            progressBar.value = Mathf.Clamp01(_currentMeal.GetCompletionRate());
    }

    private string TranslateId(string id) => id switch
    {
        "rice"     => "Gạo",
        "egg"      => "Trứng",
        "veggie"   => "Rau",
        "meat"     => "Thịt",
        "mushroom" => "Nấm",
        "spice"    => "Gia vị",
        "carrot"   => "Cà rốt",
        "noodle"   => "Mì",
        "onion"    => "Hành",
        _          => id
    };

}
