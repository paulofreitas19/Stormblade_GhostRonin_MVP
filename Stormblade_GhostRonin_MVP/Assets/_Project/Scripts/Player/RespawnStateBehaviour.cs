using UnityEngine;

public class RespawnStateBehaviour : StateMachineBehaviour
{
    private PlayerRespawnController respawnController;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(respawnController == null)
            respawnController = animator.GetComponentInParent<PlayerRespawnController>();

        if(respawnController == null)
        {
            Debug.LogError("RespawnStateBehaviour: PlayerRespawnController não encontrado.");

            return;
        }

        respawnController.BeginRespawnLock();

        Debug.Log($"[ANIMATOR] Entrou no State Respawn | frame {Time.frameCount}");
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(respawnController == null)
            return;

        Debug.Log($"[ANIMATOR] Saiu do State Respawn | frame {Time.frameCount}");

        respawnController.FinishRespawnLock();
    }

}
