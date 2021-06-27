namespace ML.GameCommands
{
    using UnityEngine;

    public class SendOnCollisionEnter : SendGameCommand
    {
        public LayerMask layerMask;

        private void OnCollisionEnter(Collision other)
        {
            if (0 != (layerMask.value & 1 << other.gameObject.layer))
            {
                Send();
            }
        }
    }
}