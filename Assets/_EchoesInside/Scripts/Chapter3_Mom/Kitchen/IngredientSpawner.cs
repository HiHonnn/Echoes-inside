using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Sinh nguyên liệu liên tục, đều đặn — không có nhịp nghỉ.
/// Mỗi X giây spawn 1 nguyên liệu ngẫu nhiên tại vị trí X ngẫu nhiên.
/// Tốc độ rơi được kiểm soát qua gravityScale ghi đè lên Prefab.
/// </summary>
public class IngredientSpawner : MonoBehaviour
{
    [Header("Prefab nguyên liệu")]
    [Tooltip("Kéo tất cả Prefab nguyên liệu vào đây.")]
    [SerializeField] private List<GameObject> ingredientPrefabs;

    [Header("Tốc độ spawn (giây/nguyên liệu)")]
    [Tooltip("Khoảng thời gian giữa 2 lần spawn liên tiếp.\nThấp = nhanh hơn, cao = chậm hơn.")]
    [SerializeField] private float spawnInterval = 1.5f;

    [Header("Vị trí spawn")]
    [Tooltip("Chiều cao Y để spawn (phía trên màn hình)")]
    [SerializeField] private float spawnY = 5.5f;

    [Tooltip("Phạm vi X ngẫu nhiên")]
    [SerializeField] private float minX = -4f;
    [SerializeField] private float maxX = 4f;

    [Header("Tốc độ rơi")]
    [Tooltip("Gravity Scale ghi đè lên Prefab.\nThấp = rơi chậm (0.5 ~ 1.0 là phù hợp)")]
    [SerializeField] private float gravityScale = 0.6f;

    private bool _isRunning = false;

    private void OnValidate()
    {
        if (ingredientPrefabs == null || ingredientPrefabs.Count == 0)
        {
            Debug.LogWarning("[IngredientSpawner] Chưa cấu hình Ingredient Prefab.", this);
        }
        else
        {
            for (int i = 0; i < ingredientPrefabs.Count; i++)
            {
                if (ingredientPrefabs[i] == null)
                    Debug.LogWarning($"[IngredientSpawner] Prefab tại index {i} chưa được gán.", this);
            }
        }

        if (spawnInterval <= 0f)
            Debug.LogWarning("[IngredientSpawner] Spawn Interval phải lớn hơn 0.", this);

        if (minX >= maxX)
            Debug.LogWarning("[IngredientSpawner] Min X phải nhỏ hơn Max X.", this);

        if (gravityScale <= 0f)
            Debug.LogWarning("[IngredientSpawner] Gravity Scale phải lớn hơn 0.", this);
    }

    // ── Điều khiển từ MealGameManager ────────────────────────
    public void StartSpawning()
    {
        if (_isRunning) return;
        _isRunning = true;
        StartCoroutine(SpawnLoop());
        Debug.Log("[IngredientSpawner] Bắt đầu sinh nguyên liệu liên tục.");
    }

    public void StopSpawning()
    {
        _isRunning = false;
        StopAllCoroutines();
        Debug.Log("[IngredientSpawner] Dừng sinh nguyên liệu.");
    }

    // ── Vòng lặp liên tục, không nghỉ ───────────────────────
    private IEnumerator SpawnLoop()
    {
        while (_isRunning)
        {
            SpawnOne();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnOne()
    {
        if (ingredientPrefabs == null || ingredientPrefabs.Count == 0)
        {
            Debug.LogWarning("[IngredientSpawner] Danh sách Prefab trống!");
            return;
        }

        int idx = Random.Range(0, ingredientPrefabs.Count);
        float x = Random.Range(minX, maxX);
        Vector3 pos = new Vector3(x, spawnY, 0f);

        GameObject obj = Instantiate(ingredientPrefabs[idx], pos, Quaternion.identity);

        // Ghi đè tốc độ rơi
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.gravityScale = gravityScale;
    }
}
