namespace ML.GameCommands
{
    using UnityEngine;

    public class SendOnBecameVisible : SendGameCommand
    {
        private void OnBecameVisible()
        {
            Send();
        }
    }
}