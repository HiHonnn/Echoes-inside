using Unity.Cinemachine;
using UnityEngine;

public class MapTransation : MonoBehaviour
{
    [Header("Map Transition")]
    [SerializeField] private PolygonCollider2D mapBoundry;
    [SerializeField] private Vector2 teleportPosition;

    [Tooltip("Offset applied after teleporting. Use a negative Y value to place the player below the destination doorway.")]
    [SerializeField] private Vector2 arrivalOffset = new Vector2(0f, -0.75f);

    private CinemachineConfiner2D confiner;
    private ScreenFader screenFader;
    private bool isTransitioning;

    private void Awake()
    {
        confiner = FindFirstObjectByType<CinemachineConfiner2D>();
        screenFader = FindFirstObjectByType<ScreenFader>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.CompareTag("Player") || isTransitioning)
        {
            return;
        }

        if (screenFader == null)
        {
            screenFader = FindFirstObjectByType<ScreenFader>();
        }

        if (screenFader == null)
        {
            ApplyTransition(collision);
            return;
        }

        BeginFadeTransition(collision);
    }

    private void BeginFadeTransition(Collider2D collision)
    {
        isTransitioning = true;

        Rigidbody2D playerRigidbody = collision.attachedRigidbody;
        Transform playerTransform = playerRigidbody != null
            ? playerRigidbody.transform
            : collision.transform;
        PlayerController playerController = playerTransform.GetComponent<PlayerController>();
        bool shouldRestorePlayerControl = playerController != null && playerController.enabled;

        if (playerController != null)
        {
            playerController.StopMovementAndSetIdle();

            if (shouldRestorePlayerControl)
            {
                playerController.enabled = false;
            }
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
        }

        screenFader.FadeOut(() =>
        {
            ApplyTransition(collision);
            screenFader.FadeIn(() =>
            {
                if (playerController != null && shouldRestorePlayerControl)
                {
                    playerController.enabled = true;
                }

                isTransitioning = false;
            });
        });
    }

    private void ApplyTransition(Collider2D collision)
    {

        if (confiner != null && mapBoundry != null)
        {
            confiner.BoundingShape2D = mapBoundry;
            confiner.InvalidateBoundingShapeCache();
        }

        Rigidbody2D playerRigidbody = collision.attachedRigidbody;
        Transform playerTransform = playerRigidbody != null
            ? playerRigidbody.transform
            : collision.transform;

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
        }

        Vector2 arrivalPosition = teleportPosition + arrivalOffset;
        playerTransform.position = new Vector3(
            arrivalPosition.x,
            arrivalPosition.y,
            playerTransform.position.z);
    }
}
