using Unity.Cinemachine;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Sprite Direction")]
    [Tooltip("Ảnh gốc của nhân vật đang nhìn về hướng nào?\nTích chọn = nhìn PHẢI (mặc định)\nBỏ tích = nhìn TRÁI")]
    [SerializeField] private bool spriteDefaultFacingRight = false;

    private Rigidbody2D _rb;
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private Vector2 _moveInput;

    // Hash các parameter của Animator để tối ưu hiệu năng
    private static readonly int IsWalkingDownHash = Animator.StringToHash("isWalkingDown");
    private static readonly int IsWalkingUpHash   = Animator.StringToHash("isWalkingUp");
    private static readonly int IsWalkingSideHash = Animator.StringToHash("isWalkingSide");

    private void Awake()
    {
        _rb             = GetComponent<Rigidbody2D>();
        _animator       = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        // Khôi phục vị trí nếu có lưu trước đó trong Scene này
        if (GameManager.Instance != null && GameManager.Instance.hasSavedPosition)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == GameManager.Instance.savedSceneName)
            {
                transform.position = GameManager.Instance.lastMainScenePosition;
                RestoreCameraBoundary();
                GameManager.Instance.hasSavedPosition = false; // Reset cờ sau khi khôi phục
                Debug.Log($"[PlayerController] Đã khôi phục vị trí Player về: {transform.position}");
            }
        }
    }

    private void RestoreCameraBoundary()
    {
        CinemachineConfiner2D confiner = FindFirstObjectByType<CinemachineConfiner2D>();
        if (confiner == null)
        {
            return;
        }

        PolygonCollider2D[] boundaries = FindObjectsByType<PolygonCollider2D>(FindObjectsSortMode.None);
        foreach (PolygonCollider2D boundary in boundaries)
        {
            if (!boundary.enabled || !boundary.OverlapPoint(transform.position))
            {
                continue;
            }

            confiner.BoundingShape2D = boundary;
            confiner.InvalidateBoundingShapeCache();
            return;
        }
    }

    public void StopMovementAndSetIdle()
    {
        _moveInput = Vector2.zero;

        if (_rb != null)
        {
            _rb.linearVelocity = Vector2.zero;
        }

        if (_animator != null)
        {
            _animator.SetBool(IsWalkingDownHash, false);
            _animator.SetBool(IsWalkingUpHash, false);
            _animator.SetBool(IsWalkingSideHash, false);
        }
    }

    private void Update()
    {
        // Nhận input di chuyển
        _moveInput.x = Input.GetAxisRaw("Horizontal");
        _moveInput.y = Input.GetAxisRaw("Vertical");

        if (_moveInput.sqrMagnitude > 1)
        {
            _moveInput.Normalize();
        }

        // Cập nhật trạng thái animation
        if (_animator != null)
        {
            UpdateAnimationStates();
        }

        // Lật hướng nhân vật bằng SpriteRenderer.flipX
        // Dùng flipX thay vì localScale để tránh bị animation clip ghi đè scale
        if (_spriteRenderer != null)
        {
            if (_moveInput.x > 0.1f)
            {
                // Đi phải: flipX = false nếu ảnh gốc nhìn phải, true nếu ảnh gốc nhìn trái
                _spriteRenderer.flipX = !spriteDefaultFacingRight;
            }
            else if (_moveInput.x < -0.1f)
            {
                // Đi trái: ngược lại với đi phải
                _spriteRenderer.flipX = spriteDefaultFacingRight;
            }
        }
    }

    private void FixedUpdate()
    {
        // Di chuyển vật lý
        _rb.linearVelocity = _moveInput * moveSpeed;
    }

    private void UpdateAnimationStates()
    {
        bool isMoving = _moveInput.magnitude > 0.1f;

        bool walkDown = false;
        bool walkUp   = false;
        bool walkSide = false;

        if (isMoving)
        {
            // So sánh chiều ngang (X) và dọc (Y) để chọn animation phù hợp
            if (Mathf.Abs(_moveInput.x) > Mathf.Abs(_moveInput.y))
            {
                walkSide = true;
            }
            else
            {
                if (_moveInput.y > 0.1f)
                {
                    walkUp = true;
                }
                else if (_moveInput.y < -0.1f)
                {
                    walkDown = true;
                }
            }
        }

        _animator.SetBool(IsWalkingDownHash, walkDown);
        _animator.SetBool(IsWalkingUpHash,   walkUp);
        _animator.SetBool(IsWalkingSideHash, walkSide);
    }
}
