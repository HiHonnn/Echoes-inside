using System.Collections;
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    [Tooltip("Danh sách các điểm tuần tra theo thứ tự")]
    public Transform[] patrolPoints;

    [Tooltip("Tốc độ di chuyển của NPC")]
    public float moveSpeed = 2f;

    [Tooltip("Khoảng cách để coi là đã tới điểm tiếp theo")]
    public float waypointReachDistance = 0.2f;

    [Tooltip("Thời gian đứng chờ tại mỗi điểm")]
    public float waitTimeAtPoint = 0.5f;

    [Header("Player Detection")]
    [Tooltip("Bật/tắt tính năng phát hiện Player")]
    public bool detectPlayer = true;

    [Tooltip("Bán kính phát hiện Player")]
    public float detectionRadius = 3f;

    [Tooltip("Tag của Player")]
    public string playerTag = "Player";

    [Header("On Catch Player")]
    [Tooltip("Kéo GameManager hoặc object xử lý khi bắt được Player")]
    public UnityEngine.Events.UnityEvent onPlayerCaught;

    [Tooltip("Thời gian chờ (giây) trước khi quái tiếp tục di chuyển sau khi bắt Player\nNên bằng Respawn Delay trong MazeGameManager")]
    public float freezeDuration = 2f;

    // ── Private state ──
    private int     _currentPointIndex = 0;
    private bool    _waiting           = false;
    private float   _waitTimer         = 0f;
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;
    private bool    _playerCaught      = false;

    private void OnValidate()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogWarning($"[EnemyPatrol] '{name}' chưa có Patrol Point.", this);
            return;
        }

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null)
                Debug.LogWarning($"[EnemyPatrol] '{name}' thiếu Patrol Point tại index {i}.", this);
        }

        if (moveSpeed <= 0f)
            Debug.LogWarning($"[EnemyPatrol] '{name}' có Move Speed không hợp lệ.", this);

        if (detectionRadius <= 0f && detectPlayer)
            Debug.LogWarning($"[EnemyPatrol] '{name}' có Detection Radius không hợp lệ.", this);
    }

    // ─────────────────────────────────────────
    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogWarning($"[EnemyPatrol] '{name}' chưa có patrol points! Hãy gán trong Inspector.");
            enabled = false;
            return;
        }

        // Teleport tới điểm đầu tiên ngay khi bắt đầu
        transform.position = patrolPoints[0].position;
    }

    void Update()
    {
        if (_playerCaught) return;

        // Kiểm tra phát hiện Player
        if (detectPlayer) CheckPlayerDetection();

        // Xử lý chờ tại điểm
        if (_waiting)
        {
            _waitTimer -= Time.deltaTime;
            if (_waitTimer <= 0f)
            {
                _waiting = false;
                // Chuyển sang điểm tiếp theo (vòng lặp)
                _currentPointIndex = (_currentPointIndex + 1) % patrolPoints.Length;
            }
            return;
        }

        MoveToCurrentPoint();
    }

    // ─────────────────────────────────────────
    void MoveToCurrentPoint()
    {
        Transform target = patrolPoints[_currentPointIndex];
        Vector2 direction = (target.position - transform.position).normalized;
        float distance    = Vector2.Distance(transform.position, target.position);

        // Di chuyển bằng Rigidbody để Physics hoạt động đúng
        _rb.linearVelocity = direction * moveSpeed;

        // Lật sprite theo hướng di chuyển (trái/phải)
        if (_sr != null && Mathf.Abs(direction.x) > 0.01f)
            _sr.flipX = direction.x < 0;

        // Kiểm tra đã tới điểm chưa
        if (distance <= waypointReachDistance)
        {
            _rb.linearVelocity = Vector2.zero;
            _waiting    = true;
            _waitTimer  = waitTimeAtPoint;
        }
    }

    // ─────────────────────────────────────────
    void CheckPlayerDetection()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectionRadius,
                             LayerMask.GetMask("Player"));

        if (hit != null && hit.CompareTag(playerTag))
        {
            CatchPlayer();
        }
    }

    // ── Chạm trực tiếp vào Player → bắt ─────
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            CatchPlayer();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            CatchPlayer();
    }

    // ─────────────────────────────────────────
    void CatchPlayer()
    {
        if (_playerCaught) return;
        _playerCaught      = true;
        _rb.linearVelocity = Vector2.zero;

        Debug.Log("[EnemyPatrol] Player bị bắt!");
        onPlayerCaught?.Invoke();

        // Tự động tiếp tục tuần tra sau khi Player hồi sinh
        StartCoroutine(ResumePatrolAfterDelay());
    }

    private IEnumerator ResumePatrolAfterDelay()
    {
        yield return new WaitForSeconds(freezeDuration);
        _playerCaught = false;
        Debug.Log("[EnemyPatrol] Tiếp tục tuần tra.");
    }

    // ─────────────────────────────────────────
    // Vẽ vòng tròn phát hiện trong Scene view (chỉ hiện khi chọn object)
    void OnDrawGizmosSelected()
    {
        if (!detectPlayer) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (patrolPoints == null) return;
        Gizmos.color = Color.yellow;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null) continue;
            Gizmos.DrawSphere(patrolPoints[i].position, 0.15f);
            int next = (i + 1) % patrolPoints.Length;
            if (patrolPoints[next] != null)
                Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[next].position);
        }
    }
}
