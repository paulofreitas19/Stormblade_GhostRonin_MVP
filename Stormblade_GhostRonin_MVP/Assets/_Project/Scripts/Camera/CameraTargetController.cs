using UnityEngine;

public class CameraTargetController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Camera mainCamera;

    [Header("Horizontal Progression")]
    [SerializeField] float forwardActivationViewportX = 0.8f;
    [SerializeField] float backwardLimitViewportX = 0.10f;

    private float targetX;
    private float fixedY;
    private float fixedZ;
    private float previousPlayerX;

    public bool IsBlockingBackwardMovement { get; private set; }

    private void Awake()
    {
        targetX = transform.position.x;
        fixedY = transform.position.y;
        fixedZ = transform.position.z;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(player != null)
            previousPlayerX = player.position.x;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (player == null || mainCamera == null)
            return;

        Vector3 playerViewportPosition = mainCamera.WorldToViewportPoint(player.position);

        IsBlockingBackwardMovement = playerViewportPosition.x <= backwardLimitViewportX;

        float playerDeltaX = player.position.x - previousPlayerX;

        bool reachedForwardLimit = playerViewportPosition.x >= forwardActivationViewportX;
        bool playerMovedForward = playerDeltaX > 0f;

        if(reachedForwardLimit && playerMovedForward)
            targetX += playerDeltaX;

        transform.position = new Vector3(targetX, fixedY, fixedZ);

        previousPlayerX = player.position.x;

    }

    public void ResetAfterRespawn(Vector3 playerPosition)
    {
        targetX = playerPosition.x;

        transform.position = new Vector3(targetX, fixedY, fixedZ);

        previousPlayerX = playerPosition.x;

        IsBlockingBackwardMovement = false;
    }
}
