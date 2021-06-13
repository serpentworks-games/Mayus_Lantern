namespace ML.GameCommands
{
    using UnityEngine;

    public class SendOnCollisionExit : SendGameCommand
    {
        public LayerMask layerMask;

        private void OnCollisionExit(Collision other)
        {
            if (0 != (layerMask.value & 1 << other.gameObject.layer))
            {
                Send();
            }
        }
    }
}