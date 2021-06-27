namespace ML.Core.MessageSystem
{
    public enum MessageType
    {
        DAMAGED,
        DEAD,
        RESPAWN,
        STUN,
    }

    public interface IMessageReceiver
    {
        void OnReceiveMessage(MessageType type, object sender, object msg);
    }
}