namespace ML.GameCommands
{
    using UnityEngine;

    [SelectionBase]
    [RequireComponent(typeof(GameCommandReceiver))]
    public abstract class GameCommandHandler : MonoBehaviour
    {

        [Tooltip("Type of Interaction to perform, in order to differentiate between different types")]
        public GameCommandType interactionType;
        [Tooltip("Is this sent only once?")]
        public bool isOneShot = false;
        [Tooltip("When not 0, the interaction will be sent once every X seconds")]
        public float interactionCoolDown = 0;
        [Tooltip("Delay in seconds before the interaction is sent to the target. Useful for needing to wait for cinematics or animations to play")]
        public float interactionStartDelay = 0;

        protected bool isTriggered = false;
        float startTime = 0;

        /// <summary>
        /// Implement this in a subclass to define the action to do
        /// </summary>
        public abstract void PerformInteraction();

        public virtual void OnInteraction()
        {
            if (isOneShot && isTriggered) return;
            isTriggered = true;
            if (interactionCoolDown > 0)
            {
                if (Time.time > startTime + interactionCoolDown)
                {
                    startTime = Time.time + interactionStartDelay;
                    ExecuteInteraction();
                }
            }
            else
            {
                ExecuteInteraction();
            }
        }

        void ExecuteInteraction()
        {
            if (interactionStartDelay > 0)
            {
                Invoke("PerformInteraction", interactionStartDelay);
            }
            else
            {
                PerformInteraction();
            }
        }

        protected virtual void Awake()
        {
            GetComponent<GameCommandReceiver>().Register(interactionType, this);
        }
    }
}