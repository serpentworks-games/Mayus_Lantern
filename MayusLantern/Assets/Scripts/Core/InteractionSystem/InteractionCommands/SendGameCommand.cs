namespace ML.GameCommands
{
    using UnityEngine;

    /// <summary>
    /// Base class for sending commands on different events
    /// </summary>
    [SelectionBase]
    public class SendGameCommand : MonoBehaviour
    {
        [Tooltip("The type of command to send. Should be reflected in the GameCommandHandler this will talk to")]
        public GameCommandType interactionType;
        [Tooltip("The object to receive the command")]
        public GameCommandReceiver interactiveObject;
        [Tooltip("When true, the command will only send once")]
        public bool isOneShot = false;
        [Tooltip("When not 0, this command will send every X seconds")]
        public float interactionCoolDown = 1;
        [Tooltip("If not null, this sound will play when the command is sent")]
        public AudioSource onSendAudio;
        [Tooltip("If onSendAudio is not null, it will play after X seconds")]
        public float audioDelay;

        float lastSendTime;
        bool isTriggered = false;

        // TODO: Name this better?? At least its not TEMPERATURE
        public float TimeSinceLastSend
        {
            get
            {
                return 1f - Mathf.Clamp01(Time.time - lastSendTime);
            }
        }

        [ContextMenu("Send Interaction")]
        public void Send()
        {
            if (isOneShot && isTriggered) return;
            if (Time.time - lastSendTime < interactionCoolDown) return;
            isTriggered = true;
            lastSendTime = Time.time;
            interactiveObject.Receive(interactionType);
            if (onSendAudio) onSendAudio.PlayDelayed(audioDelay);
        }

        protected virtual void Reset()
        {
            interactiveObject = GetComponent<GameCommandReceiver>();
        }
    }
}