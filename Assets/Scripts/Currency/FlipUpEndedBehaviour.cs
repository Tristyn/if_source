using UnityEngine;

public sealed class FlipUpEndedBehaviour : StateMachineBehaviour
{
    Currency currency;

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (currency == null)
        {
            currency = animator.GetComponent<Currency>();
        }
        currency.FlipUpEnded();
    }
}