using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerInputReader : MonoBehaviour
{
    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference attackAction;
    [SerializeField] private InputActionReference crouchAction;
    [SerializeField] private InputActionReference specialAction;

    [Header("Debug")]
    [SerializeField] private float moveInputX;
    [SerializeField] private bool jumpRequested;
    [SerializeField] private bool attackRequested;
    [SerializeField] private bool specialRequested;
    [SerializeField] private bool isCrouchHeld;

    [Header("Input State")]
    [SerializeField] private bool gameplayInputBlocked;

    public bool GameplayInputBlocked => gameplayInputBlocked;
    public float MoveInputX => moveInputX;
    public bool JumpRequested => jumpRequested;
    public bool AttackRequested => attackRequested;
    public bool SpecialRequested => specialRequested;
    public bool IsCrouchHeld => isCrouchHeld; 

    private void OnEnable()
    {
        if(moveAction != null)
        {
            moveAction.action.Enable();
        }

        if(jumpAction != null)
        {
            jumpAction.action.Enable();
        }

        if (attackAction != null)
        {
            attackAction.action.Enable();
        }

        if (crouchAction != null)
        {
            crouchAction.action.Enable();
        }

        if (specialAction != null)
        {
            specialAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if(moveAction != null)
        {
            moveAction.action.Disable();
        }

        if (jumpAction != null)
        {
            jumpAction.action.Disable();
        }

        if (attackAction != null)
        {
            attackAction.action.Disable();
        }

        if (crouchAction != null)
        {
            crouchAction.action.Disable();
        }

        if (specialAction != null)
        {
            specialAction.action.Disable();
        }
    }

    private void Update()
    {
        if (gameplayInputBlocked)
        {
            ClearGameplayInputState();
            return;
        }
        
        ReadMoveInput();
        ReadJumpInput();
        ReadAttackInput();
        ReadSpecialInput();
        ReadCrouchInput();
    }

    private void ReadMoveInput()
    {
        if (moveAction == null)
        {
            moveInputX = 0f;
            return;
        }

        Vector2 moveValue = moveAction.action.ReadValue<Vector2>();
        moveInputX = moveValue.x;
    }

    private void ReadJumpInput()
    {
        if (jumpAction == null)
        {
            jumpRequested = false;
            return;
        }

        if (jumpAction.action.WasPressedThisFrame())
        {
            jumpRequested = true;
        }

    }

    private void ReadAttackInput()
    {
        if (attackAction == null)
        {
            attackRequested = false;
            return;
        }

        if (attackAction.action.WasPressedThisFrame())
        {
            attackRequested = true;
            Debug.Log("Pedido de ataque registrado.");
        }
    }

    private void ReadSpecialInput()
    {
        if(specialAction == null)
        {
            specialRequested = false;
            return;
        }

        if (specialAction.action.WasPressedThisFrame())
        {
            specialRequested = true;
            Debug.Log("Pedido de modo energizado/especial registrado.");
        }
    }

    private void ReadCrouchInput()
    {
        if (crouchAction == null)
        {
            isCrouchHeld = false;
            return;
        }

        isCrouchHeld = crouchAction.action.IsPressed();

        if (isCrouchHeld)
        {
            Debug.LogWarning("Agachado");
        }
        
    }

    public void ConsumeJumpRequest()
    {
        jumpRequested = false;
    }

    public void ConsumeAttackRequest()
    {
        attackRequested = false;
    }

    public void ConsumeSpecialRequest()
    {
        specialRequested = false;
    }

    public void ClearActionRequests()
    {
        jumpRequested = false;
        attackRequested = false;
    }

    public void SetGamePlayInputBlocked(bool blocked)
    {
        gameplayInputBlocked = blocked;

        if(blocked)
            ClearGameplayInputState();
    }

    private void ClearGameplayInputState(){
        moveInputX = 0f;
        jumpRequested = false;
        attackRequested = false;
        specialRequested = false;
        isCrouchHeld = false;
    }

}
