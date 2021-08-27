namespace ML.Characters.Enemies
{
    using UnityEngine;

    public class WandererController : AIController
    {
        private void Update()
        {
            //Stunned behaviour
            if (combat.isAlerted)
            {
                player = combat.target;
                AttackBehaviour();
            }
            else if (HasAggro() && combat.CanAttack(player))
            {
                AttackBehaviour();
            }
            else if (CanSeePlayer() && combat.CanAttack(player))
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
}