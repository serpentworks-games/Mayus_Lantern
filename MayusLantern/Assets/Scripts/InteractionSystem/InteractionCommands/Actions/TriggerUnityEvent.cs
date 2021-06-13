namespace ML.GameCommands
{
    using UnityEngine.Events;

    public class TriggerUnityEvent : GameCommandHandler
    {

        public UnityEvent unityEvent;

        public override void PerformInteraction()
        {
            unityEvent.Invoke();
        }
    }
}