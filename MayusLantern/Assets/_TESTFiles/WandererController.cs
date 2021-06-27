using UnityEngine;

public class WandererController : AIController
{
    private void Update()
    {
        //Stunned behaviour

        if (CanSeePlayer() && combat.CanAttack(player))
        {
            AttackBehaviour();
        }
        else if (timeSinceLastSawPlayer < patrolInfo.suspicionTime)
        {
            SuspicionBehaviour();
        }
        else
        {
            PatrolBehaviour();
        }

        UpdateTimers();
    }

}