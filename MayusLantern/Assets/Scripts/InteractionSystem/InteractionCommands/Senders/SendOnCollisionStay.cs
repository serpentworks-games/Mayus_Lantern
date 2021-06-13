namespace ML.GameCommands
{
    using UnityEngine;

    public class SendOnCollisionStay : SendGameCommand
    {
        public LayerMask layerMask;

        private void OnCollisionStay(Collision other)
        {
            if (0 != (layerMask.value & 1 << other.gameObject.layer))
            {
                Send();
            }
        }
    }
}