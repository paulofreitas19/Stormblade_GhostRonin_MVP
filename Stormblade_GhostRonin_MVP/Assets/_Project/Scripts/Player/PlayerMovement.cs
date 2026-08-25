using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private PlayerInputReader inputReader;
    [SerializeField] private Transform visual;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private CameraTargetController cameraTargetController;
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private Health health;

    [Header("Body Collider")]
    [SerializeField] private CapsuleCollider2D bodyCollider;
    [SerializeField] private Vector2 standingColliderSize;
    [SerializeField] private Vector2 standingColliderOffset;
    [SerializeField] private Vector2 crouchingColliderSize;
    [SerializeField] private Vector2 crouchingColliderOffset;

    [Header("Hurtbox")]
    [SerializeField] private BoxCollider2D hurtboxCollider;
    [SerializeField] private Vector2 standingHurtboxSize;
    [SerializeField] private Vector2 standingHurtboxOffset;
    [SerializeField] private Vector2 crouchingHurtboxSize;
    [SerializeField] private Vector2 crouchingHurtboxOffset;

    [Header("Horizontal Movement")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Vertical Movement")]
    [SerializeField] private float jumpImpulse = 12f;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Vertical State Debug")]
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool wasGroundedLastFrame;
    [SerializeField] private bool jumpStartedThisFrame;
    [SerializeField] private bool landedThisFrame;
    [SerializeField] private bool leftGroundThisFrame;

    private float moveInputX;
    private bool isFacingRight = true;

    [Header("Crouch State")]
    [SerializeField]private bool isCrouching;
    [SerializeField] private bool enteredCrouchThisFrame;
    [SerializeField] private bool wasCrouchingLastFrame;

    [SerializeField] private bool wasAirAttackActiveLastFrame;
    [SerializeField] private int airAttackLockedDirection;
    [SerializeField] private float airAttackLockedSpeedX;

    [Header("Ceiling Check")]
    [SerializeField] private Transform ceilingCheck;
    [SerializeField] private float ceilingCheckRadius = 0.1f;
    [SerializeField] private LayerMask ceilingLayer;

    [Header("Ceiling State Debug")]
    [SerializeField] private bool isTouchingCeiling;
    [SerializeField] private bool hitCeilingThisFrame;

    [Header("Damage Pushback State")]
    [SerializeField] private bool isDamagePushbackActive;

    private float damagePushbackVelocityX;
    private float damagePushbackTimer;

    public bool IsDamagePushbackActive => isDamagePushbackActive;
    private bool airborneStartedByJump;
    public bool AirborneStartedByJump => airborneStartedByJump;
    public bool IsMovingHorizontally => Mathf.Abs(moveInputX) > 0.01f;
    public bool IsFacingRight => isFacingRight;
    public bool IsGrounded => isGrounded;
    public float VerticalVelocity => rb != null ? rb.linearVelocity.y : 0f;
    public bool IsRising => VerticalVelocity > 0.01f;
    public bool IsFalling => VerticalVelocity < -0.01f;
    public bool ShouldPlayFallAnimation => !isGrounded && !airborneStartedByJump;
    public bool JumpStartedThisFrame => jumpStartedThisFrame;
    public bool LandedThisFrame => landedThisFrame;
    public bool HasJumpRequest => inputReader != null && inputReader.JumpRequested;
    public bool LeftGroundThisFrame => leftGroundThisFrame;
    public bool IsAirborne => !isGrounded;
    public bool IsCrouching => isCrouching;
    public bool IsTouchingCeiling => isTouchingCeiling;
    public bool HitCeilingThisFrame => hitCeilingThisFrame;

    public bool IsDead()
    {
        return health != null && health.IsDead;
    }

    private void Start()
    {
        ApplyStandingBodyCollider();
        ApplyStandingHurtbox();
        wasCrouchingLastFrame = false;
    }

    private void Update()
    {
        if (inputReader == null)
        {
            moveInputX = 0f;
            return;
        }

        if (IsDead())
        {
            StopMovementOnDeath();
            UpdateBodyColliderForCrouch();
            UpdateHurtboxForCrouch();
            return;
        }

        moveInputX = inputReader.MoveInputX;
 
        UpdateCrouchState();
        UpdateBodyColliderForCrouch();
        UpdateHurtboxForCrouch();
        HandleFacingDirection();
    }

    private void FixedUpdate()
    {
        ResetFrameFlags();
        CheckGround();
        CheckCeiling();

        if (rb == null)
            return;

        if (IsDead())
        {
            StopMovementOnDeath();
            return;
        }

        if(HandleDamagePushback())
            return;

        if (enteredCrouchThisFrame)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        UpdateAirAttackLockState();
        HandleHorizontalMovement();
        HandleJump();
    }

    private void UpdateAirAttackLockState()
    {
        bool isAirAttackActive = playerCombat != null && playerCombat.IsAirAttackActive;

        if (isAirAttackActive && !wasAirAttackActiveLastFrame)
        {
            airAttackLockedDirection = isFacingRight ? 1 : -1;
            airAttackLockedSpeedX = rb != null ? rb.linearVelocity.x : 0f;

            if (airAttackLockedDirection > 0)
                airAttackLockedSpeedX = Mathf.Max(0f, airAttackLockedSpeedX);

            else
                airAttackLockedSpeedX = Mathf.Min(0f, airAttackLockedSpeedX);
        }

        if (landedThisFrame)
        {
            airAttackLockedDirection = 0;
            airAttackLockedSpeedX = 0f;
        }

        wasAirAttackActiveLastFrame = isAirAttackActive;
    }

    private float GetFilteredMoveInputX()
    {
        float filteredMoveInputX = moveInputX;

        if (cameraTargetController != null && cameraTargetController.IsBlockingBackwardMovement && filteredMoveInputX < 0f)
        {
            filteredMoveInputX = 0f;
        }

        return filteredMoveInputX;
    }

    private void HandleHorizontalMovement()
    {
        if(airAttackLockedDirection != 0 && !isGrounded)
        {
            HandleAirAttackHorizontalMovement();
            return;
        }

        if (playerCombat != null && playerCombat.IsAttacking)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if (isCrouching)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        float filteredMoveInputX = GetFilteredMoveInputX();

        rb.linearVelocity = new Vector2(filteredMoveInputX * moveSpeed, rb.linearVelocity.y);
    }

    private void HandleAirAttackHorizontalMovement()
    {
        if (rb == null)
            return;

        float filteredMoveInputX = GetFilteredMoveInputX();

        if (airAttackLockedDirection == 0)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        if ((airAttackLockedDirection > 0 && filteredMoveInputX < 0f) || (airAttackLockedDirection < 0 && filteredMoveInputX > 0f))
            filteredMoveInputX = 0f;

        float targetX;

        if (Mathf.Abs(filteredMoveInputX) > 0.01f)
            targetX = filteredMoveInputX * moveSpeed;

        else
            targetX = airAttackLockedSpeedX;

        if (airAttackLockedDirection > 0)
            targetX = Mathf.Max(0f, targetX);

        else
            targetX = Mathf.Min(0f, targetX);

        airAttackLockedSpeedX = targetX;
        rb.linearVelocity = new Vector2(targetX, rb.linearVelocity.y);
    }

    private void StopMovementOnDeath()
    {
        moveInputX = 0f;
        isCrouching = false;
        enteredCrouchThisFrame = false;

        isDamagePushbackActive = false;
        damagePushbackTimer = 0f;
        damagePushbackVelocityX = 0f;

        if (rb != null)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    private void ResetFrameFlags()
    {
        jumpStartedThisFrame = false;
        landedThisFrame = false;
        leftGroundThisFrame = false;
        enteredCrouchThisFrame = false;
        hitCeilingThisFrame = false;
    }

    private void CheckGround()
    {
        wasGroundedLastFrame = isGrounded;

        if (groundCheck == null)
        {
            isGrounded = false;
            landedThisFrame = false;
            return;
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        leftGroundThisFrame = wasGroundedLastFrame && !isGrounded;
        landedThisFrame = !wasGroundedLastFrame && isGrounded;

        if (landedThisFrame)
            airborneStartedByJump = false;
    }

    private void CheckCeiling()
    {
        if(ceilingCheck == null)
        {
            isTouchingCeiling = false;
            return;
        }

        isTouchingCeiling = Physics2D.OverlapCircle(ceilingCheck.position, ceilingCheckRadius, ceilingLayer);

        if (!isTouchingCeiling)
            return;

        if (isGrounded)
            return;

        if (!airborneStartedByJump)
            return;

        hitCeilingThisFrame = true;
        airborneStartedByJump = false;

        if (rb != null && rb.linearVelocity.y > 0f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

        Debug.Log("Player bateu no teto. Estado aéreo alterado para queda.");
    }

    void HandleFacingDirection()
    {
        if (visual == null)
            return;

        if (IsDead())
            return;

        if (playerCombat != null && playerCombat.IsAttacking)
            return;

        if(airAttackLockedDirection != 0 && !isGrounded)
            return;

        if (isCrouching)
            return;

        if(moveInputX > 0.01f && !isFacingRight)
        {
            Flip(true);
        }

        else if(moveInputX < -0.01f && isFacingRight)
        {
            Flip(false);
        }
    }

    void Flip(bool faceRight)
    {
        isFacingRight = faceRight;

        if (visual == null)
            return;

        Vector3 scale = visual.localScale;
        scale.x = faceRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        visual.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if(ceilingCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(ceilingCheck.position, ceilingCheckRadius);
        }
    }

    private void HandleJump()
    {
        if (inputReader == null || rb == null)
            return;

        if (IsDead())
        {
            inputReader.ConsumeJumpRequest();
            return;
        }

        if (!inputReader.JumpRequested)
            return;

        if (isCrouching)
        {
            inputReader.ConsumeJumpRequest();
            return;
        }

        if(playerCombat != null && playerCombat.IsBasicAttackActive || playerCombat != null && playerCombat.IsCrouchAttackActive)
        {
            inputReader.ConsumeJumpRequest();
            Debug.Log("Pulo bloqueado durante ataque básico.");
            return;
        }
            
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpImpulse);

            jumpStartedThisFrame = true;
            airborneStartedByJump = true;
        }

        inputReader.ConsumeJumpRequest();
    }

    private bool CanEnterOrStayCrouching()
    {
        if (inputReader == null)
            return false;

        if (!inputReader.IsCrouchHeld)
            return false;

        if (!isGrounded)
            return false;

        if (playerCombat != null && playerCombat.IsAttacking)
            return false;

        return true;
    }

    private void UpdateCrouchState()
    {
        if (IsDead())
        {
            isCrouching = false;
            enteredCrouchThisFrame = false;
            return;
        }

        bool wasCrouching = isCrouching;
        isCrouching = CanEnterOrStayCrouching();
        enteredCrouchThisFrame = !wasCrouching && isCrouching;
    }

    private void ApplyStandingBodyCollider()
    {
        if (bodyCollider == null)
            return;

        bodyCollider.size = standingColliderSize;
        bodyCollider.offset = standingColliderOffset;
    }

    private void ApplyCrouchingBodyCollider()
    {
        if (bodyCollider == null)
            return;

        bodyCollider.size = crouchingColliderSize;
        bodyCollider.offset = crouchingColliderOffset;
    }

    private void ApplyStandingHurtbox()
    {
        if (hurtboxCollider == null)
            return;

        hurtboxCollider.size = standingHurtboxSize;
        hurtboxCollider.offset = standingHurtboxOffset;
    }

    private void ApplyCrouchingHurtbox()
    {
        if (hurtboxCollider == null)
            return;

        hurtboxCollider.size = crouchingHurtboxSize;
        hurtboxCollider.offset = crouchingHurtboxOffset;
    }

    private void UpdateBodyColliderForCrouch()
    {
        if (bodyCollider == null)
            return;

        if (isCrouching == wasCrouchingLastFrame)
            return;

        if (isCrouching)
            ApplyCrouchingBodyCollider();

        else
            ApplyStandingBodyCollider();

        wasCrouchingLastFrame = isCrouching;
    }

    private void UpdateHurtboxForCrouch()
    {
        if (hurtboxCollider == null)
            return;

        if (isCrouching)
            ApplyCrouchingHurtbox();

        else
            ApplyStandingHurtbox();

        wasCrouchingLastFrame = isCrouching;
    }

    public void StartDamagePushback(float directionX, float speed, float duration)
    {
        if(rb == null)
            return;

        if(IsDead())
            return;

        if(Mathf.Abs(directionX) < 0.01f)
            return;

        speed = Mathf.Max(0f, speed);
        duration = Mathf.Max(0f, duration);

        if(speed <= 0f || duration <= 0f)
            return;

        damagePushbackVelocityX = Mathf.Sign(directionX) * speed;

        damagePushbackTimer = duration;
        isDamagePushbackActive = true;

        rb.linearVelocity = new Vector2(damagePushbackVelocityX, rb.linearVelocity.y);
    }

    private bool HandleDamagePushback()
    {
        if(!isDamagePushbackActive)
            return false;
        
        rb.linearVelocity = new Vector2(damagePushbackVelocityX, rb.linearVelocity.y);

        damagePushbackTimer -= Time.fixedDeltaTime;

        if(damagePushbackTimer <= 0f)
        {
            isDamagePushbackActive = false;
            damagePushbackTimer = 0f;
            damagePushbackVelocityX = 0f;
        }

        return true;
    }
}
