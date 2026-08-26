using UnityEngine;
using System;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private Health health;
    [SerializeField] private PlayerRespawnController respawnController;

    [Header("Air State Timing")]
    [SerializeField] private float jumpStartHoldTime = 0.10f;
    [SerializeField] private float jumpLandingHoldTime = 0.8f;

    public event Action OnDeathTransitionPoint;
    
    private static readonly int BaseStateHash = Animator.StringToHash("baseState");
    private static readonly int AttackHash = Animator.StringToHash("attackBasic");
    private static readonly int CrouchAttackHash = Animator.StringToHash("attackCrouch");
    private static readonly int AirAttackHash = Animator.StringToHash("attackAir");
    private static readonly int IsDeadHash = Animator.StringToHash("isDead");
    private static readonly int HitHash = Animator.StringToHash("hit");
    private static readonly int RespawnHash = Animator.StringToHash("respawn");

    private PlayerBaseState currentBaseState = PlayerBaseState.Idle;

    private bool isTransientStateActive;
    private float transientStateTimer;
    private PlayerBaseState transientState;
    private bool deathAnimationStarted;

    private void Awake()
    {
        if(respawnController == null)
            respawnController = GetComponentInParent<PlayerRespawnController>();
        if (animator == null)
            animator = GetComponent<Animator>();

        if (health == null)
            health = GetComponentInParent<Health>();
    }

    private void Update()
    {
        if(health != null && health.IsDead)
            return;
        
        if(IsRespawning())
            return;
        
        UpdateBaseState();
    }

    private void HandleDamageAnimation(DamageData damageData)
    {
        if(animator == null)
            return;

        if(health == null || health.IsDead)
            return;

        EndCurrentAttack();

        animator.SetTrigger(HitHash);
    }

    private void HandleDeathAnimation()
    {
        if (deathAnimationStarted)
            return;

        if(animator == null)
            return;

        deathAnimationStarted = true;

        animator.ResetTrigger(HitHash);
        animator.SetBool(IsDeadHash, true);
    }

    private bool ShouldInterruptLandingWithJump()
    {
        return isTransientStateActive &&
               transientState == PlayerBaseState.JumpLanding &&
               playerMovement != null &&
               playerMovement.JumpStartedThisFrame;
    }
    
    private void UpdateBaseState()
    {
        if (animator == null || playerMovement == null)
            return;

        if (playerMovement.LandedThisFrame)
            SetTransientState(PlayerBaseState.JumpLanding, jumpLandingHoldTime);

        else if (playerMovement.JumpStartedThisFrame)
            SetTransientState(PlayerBaseState.JumpStart, jumpStartHoldTime);

        // prioridade: se durante o landing o jogador j� pediu novo pulo, 
        // o landing � cancelado imediatamente
        if (ShouldInterruptLandingWithJump())
        {
            SetTransientState(PlayerBaseState.JumpStart, jumpStartHoldTime);
        }

        UpdateTransientTimer();

        PlayerBaseState targetBaseState = CalculateBaseState();

        if (targetBaseState != currentBaseState)
        {
            currentBaseState = targetBaseState;
            animator.SetInteger(BaseStateHash, (int)currentBaseState);
        }
    }

    private void UpdateTransientTimer()
    {
        if (!isTransientStateActive)
            return;

        transientStateTimer -= Time.deltaTime;

        if (transientStateTimer <= 0f)
            isTransientStateActive = false;
    }

    private void SetTransientState(PlayerBaseState state, float duration)
    {
        transientState = state;
        transientStateTimer = duration;
        isTransientStateActive = true;
    }

    private PlayerBaseState CalculateBaseState()
    {
        if (isTransientStateActive)
            return transientState;

        if (playerMovement.IsGrounded)
        {
            if (playerMovement.IsCrouching)
                return PlayerBaseState.Crouch;

            if (playerMovement.IsMovingHorizontally)
                return PlayerBaseState.Run;

            return PlayerBaseState.Idle;
        }

        if (playerMovement.ShouldPlayFallAnimation)
            return PlayerBaseState.Fall;

        return PlayerBaseState.JumpAir;
    }

    public void PlayAttack()
    {
        if (animator == null)
            return;
        
        if(IsRespawning())
            return;

        animator.SetTrigger(AttackHash);
    }

    public void EndCurrentAttack()
    {
        DisableAttackHitbox();

        if (playerCombat != null)
        {
            playerCombat.EndCurrentAttack();
        }
    }

    public void EnableAttackHitbox()
    {
        if (playerCombat != null)
        {
            playerCombat.EnableAttackHitbox();
        }
    }

    public void DisableAttackHitbox()
    {
        if (playerCombat != null)
        {
            playerCombat.DisableAttackHitbox();
        }
    }

    public void PlayCrouchAttack()
    {
        if (animator == null)
            return;

        if(IsRespawning())
            return;

        animator.SetTrigger(CrouchAttackHash);
    }

    public void PlayAirAttack()
    {
        if (animator == null)
            return;

        if(IsRespawning())
            return;

        animator.SetTrigger(AirAttackHash);
    }

    public void ResetDeathAnimation()
    {
        deathAnimationStarted = false;
        animator.SetBool(IsDeadHash, false);
    }

    private void OnEnable()
    {
        if(health == null)
            return;

        health.OnDamaged += HandleDamageAnimation;
        health.OnDied += HandleDeathAnimation;
    } 

    private void OnDisable()
    {
        if(health == null)
            return;

        health.OnDamaged -= HandleDamageAnimation;
        health.OnDied -= HandleDeathAnimation;
    }

    public void AnimationEvent_DeathTransitionPoint()
    {
        OnDeathTransitionPoint?.Invoke();
    }

    public void PlayRespawn()
    {
        if(animator == null)
            return;

        deathAnimationStarted = false;

        animator.ResetTrigger(AttackHash);
        animator.ResetTrigger(CrouchAttackHash);
        animator.ResetTrigger(AirAttackHash);
        animator.ResetTrigger(HitHash);

        animator.SetBool(IsDeadHash, false);
        animator.SetTrigger(RespawnHash);

        Debug.Log("PlayerAnimationController: Respawn iniciado.");
    }

    private bool IsRespawning()
    {
        return respawnController != null && respawnController.IsRespawning;
    }

}
