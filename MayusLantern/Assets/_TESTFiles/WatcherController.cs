using UnityEngine;

public class WatcherController : AIController
{
    private void Update()
    {
        if (CanSeePlayer() && combat.CanAlert(player))
        {
            AlertBehaviour();
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