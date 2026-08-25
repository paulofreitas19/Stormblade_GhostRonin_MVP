using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputReader playerInputReader;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerAnimationController playerAnimationController;
    [SerializeField] private Health health;

    [Header("Attack References")]
    [SerializeField] private Hitbox attackHitbox;

    [Header("Hitbox Settings")]
    [SerializeField] private CircleCollider2D hitboxCollider;
    [SerializeField] private float basicAttackHitboxRadius;
    [SerializeField] private Vector2 basicAttackHitboxOffset;
    [SerializeField] private float crouchAttackHitboxRadius;
    [SerializeField] private Vector2 crouchAttackHitboxOffset;
    [SerializeField] private float airAttackHitboxRadius;
    [SerializeField] private Vector2 airAttackHitboxOffset;

    [Header("Attack State")]
    [SerializeField] private bool isAttacking;
    [SerializeField] private bool airAttackUsedThisAirborne;

    [SerializeField] private AttackType currentAttackType = AttackType.None;

    [SerializeField] private int airAttackLockedDirection = 0;

    private Vector3 attackHitboxBaseLocalPosition;

    public Hitbox AttackHitbox => attackHitbox;
    public bool IsAttacking => isAttacking;
    public bool IsAirAttackActive => isAttacking && currentAttackType == AttackType.Air;
    public bool IsBasicAttackActive => isAttacking && currentAttackType == AttackType.Basic;
    public bool IsCrouchAttackActive => isAttacking && currentAttackType == AttackType.Crouch;
    public AttackType CurrentAttackType => currentAttackType;

    public bool IsDead()
    {
        return health != null && health.IsDead;
    }

    //belly e daniel estiveram aqui
    private void Awake()
    {
        if (attackHitbox == null)
        {
            Debug.LogWarning($"{gameObject.name}: attackHitbox n�o foi atribu�do no PlayerCombat.");
        }

        if (playerInputReader == null)
        {
            Debug.LogWarning($"{gameObject.name}: playerInputReader n�o foi atribu�do no PlayerCombat.");
        }

        if (playerMovement == null)
        {
            Debug.LogWarning($"{gameObject.name}: playerMovement n�o foi atribu�do no PlayerCombat");
        }

        if (playerAnimationController == null)
        {
            Debug.LogWarning($"{gameObject.name}: playerAnimationController n�o foi atribu�do no PlayerCombat");
        }

        DisableAttackHitbox();

        if (attackHitbox != null)
        {
            attackHitboxBaseLocalPosition = attackHitbox.transform.localPosition;
        }

        if (playerMovement != null)
        {
            UpdateAttackHitboxDirection(playerMovement.IsFacingRight);
        }
    }

    private void ForceStopCombatOnDeath()
    {
        if(attackHitbox != null)
            attackHitbox.DisableHitbox();

        bool combatWasActive = isAttacking;

        isAttacking = false;
        currentAttackType = AttackType.None;
        airAttackLockedDirection = 0;

        if(combatWasActive)
            Debug.Log("Player Combat: combate interrompido pela morte do protagonista.");
    }

    private void Update()
    {
        if (IsDead())
        {
            ForceStopCombatOnDeath();
            return;
        }

        ResetAirAttackOnLanding();

        HandleAttackRequest();

        if (playerMovement != null)
        {
            UpdateAttackHitboxDirection(playerMovement.IsFacingRight);
        }
    }

    private void ResetAirAttackOnLanding()
    {
        if(playerMovement == null)
            return;

        if(!playerMovement.LandedThisFrame)
            return;

        airAttackUsedThisAirborne = false;
    }

    private void HandleAttackRequest()
    {
        if (playerInputReader == null)
            return;

        if (IsDead())
        {
            playerInputReader.ConsumeAttackRequest();
            return;
        }

        if (!playerInputReader.AttackRequested)
            return;

        playerInputReader.ConsumeAttackRequest();

        if (isAttacking)
            return;

        TryStartContextualAttack();

        //if (!CanStartBasicAttack())
        //{
        //    Debug.Log("PlayerCombat: pedido de ataque ignorado por regra de execu��o.");
        //    return;
        //}

    }

    private bool CanStartBasicAttack()
    {
        if (isAttacking)
            return false;

        if (playerMovement == null)
            return false;

        if (!playerMovement.IsGrounded)
            return false;

        return true;
    }

    private void StartAirAttack()
    {
        if (isAttacking)
            return;

        if(airAttackUsedThisAirborne)
            return;

        ApplyHitboxShape(
            airAttackHitboxRadius,
            GetFacingAdjustedOffset(airAttackHitboxOffset)
        );

        currentAttackType = AttackType.Air;
        isAttacking = true;

        airAttackUsedThisAirborne = true;

        airAttackLockedDirection = playerMovement != null && playerMovement.IsFacingRight ? 1 : -1;

        if (playerAnimationController != null)
            playerAnimationController.PlayAirAttack();
    }

    private void StartCrouchAttack()
    {
        if (isAttacking)
            return;

        ApplyHitboxShape(
            crouchAttackHitboxRadius, 
            GetFacingAdjustedOffset(crouchAttackHitboxOffset)
        );

        currentAttackType = AttackType.Crouch;

        isAttacking = true;

        if (playerAnimationController != null)
            playerAnimationController.PlayCrouchAttack();
    }

    private void StartBasicAttack()
    {
        if (isAttacking)
            return;

        ApplyHitboxShape(
            basicAttackHitboxRadius,
            GetFacingAdjustedOffset(basicAttackHitboxOffset)
        );

        currentAttackType = AttackType.Basic;

        isAttacking = true;

        if (playerAnimationController != null)
            playerAnimationController.PlayAttack();

    }

    public void EndCurrentAttack()
    {
        if (!isAttacking)
            return;

        isAttacking = false;

        currentAttackType = AttackType.None;

        airAttackLockedDirection = 0;

        Debug.Log("PlayerCombat: ataque encerrado.");
    }

    private void TryStartContextualAttack()
    {
        if (IsInAirAttackContext())
        {
            StartAirAttack();
            return;
        }

        if (IsInCrouchAttackContext())
        {
            StartCrouchAttack();
            return;
        }

        if (IsInBasicAttackContext())
        {
            StartBasicAttack();
            return;
        }
    }

    private void UpdateAttackHitboxDirection(bool isFacingRight)
    {
        if (attackHitbox == null)
            return;

        Vector3 localPosition = attackHitboxBaseLocalPosition;
        localPosition.x = Mathf.Abs(localPosition.x) * (isFacingRight ? 1f : -1f);

        attackHitbox.transform.localPosition = localPosition;
    }

    public void EnableAttackHitbox()
    {
        if (attackHitbox != null)
        {
            attackHitbox.EnableHitbox();
        }
    }

    public void DisableAttackHitbox()
    {
        if (attackHitbox != null)
        {
            attackHitbox.DisableHitbox();
        }
    }

    private bool IsInAirAttackContext()
    {
        if (playerMovement == null)
            return false;

        return playerMovement.IsAirborne;
    }

    private bool IsInCrouchAttackContext()
    {
        if (playerMovement == null)
            return false;

        return playerMovement.IsGrounded && playerMovement.IsCrouching;
    }

    private bool IsInBasicAttackContext()
    {
        if (playerMovement == null)
            return false;

        return playerMovement.IsGrounded && !playerMovement.IsCrouching;
    }

    private void ApplyHitboxShape(float radius, Vector2 offset)
    {
        if (hitboxCollider == null)
            return;

        hitboxCollider.radius = radius;
        hitboxCollider.offset = offset;
    }

    private Vector2 GetFacingAdjustedOffset(Vector2 baseOffset)
    {
        if (playerMovement != null && !playerMovement.IsFacingRight)
            return new Vector2(-baseOffset.x, baseOffset.y);

        return baseOffset;
    }

    public enum AttackType
    {
        None,
        Basic,
        Crouch,
        Air
    }


}
