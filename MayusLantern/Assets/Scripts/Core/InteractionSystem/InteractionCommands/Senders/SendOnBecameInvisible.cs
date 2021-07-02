namespace ML.GameCommands
{
    using UnityEngine;

    public class SendOnBecameInvisible : SendGameCommand
    {
        private void OnBecameInvisible()
        {
            Send();
        }
    }
}