namespace ML.GameCommands
{
    using UnityEngine;
    public class GameplayCounter : GameCommandHandler
    {
        [Space]
        public int currentCount = 0;
        public int targetCount = 3;

        [Space]
        [Tooltip("Send a command when currentCount changes")]
        public SendGameCommand onIncrementSendCommand;
        [Tooltip("Perform an action when currentCount changes")]
        public GameCommandHandler onIncrementPerformAction;
        [Space]
        [Tooltip("Send a command when targetCount is reached")]
        public SendGameCommand onTargetReachedSendCommand;
        [Tooltip("Perform an action when targetCount is reached")]
        public GameCommandHandler onTargetReachedPerformAction;

        public override void PerformInteraction()
        {
            currentCount += 1;
            if (currentCount >= targetCount)
            {
                if (onTargetReachedPerformAction != null)
                {
                    onTargetReachedPerformAction.PerformInteraction();
                }
                if (onTargetReachedSendCommand != null)
                {
                    onTargetReachedSendCommand.Send();
                }
                isTriggered = true;
            }
            else
            {
                if (onIncrementPerformAction != null)
                {
                    onIncrementPerformAction.PerformInteraction();
                }
                if (onIncrementSendCommand != null)
                {
                    onIncrementSendCommand.Send();
                }
                isTriggered = false;
            }
        }
    }
}