using UnityEngine;
using System;

public class GameOverBackgroundTransition : MonoBehaviour
{
    [SerializeField] private Animator animator;

    public event Action OnTransitionFinished;

    private static readonly int ContinueHash = Animator.StringToHash("continue");

    public void PlayContinueTransition()
    {
        if(animator == null)
            return;

        animator.SetTrigger(ContinueHash);
    }

    public void FinishContinueTransition()
    {
        OnTransitionFinished?.Invoke();
    }
}
