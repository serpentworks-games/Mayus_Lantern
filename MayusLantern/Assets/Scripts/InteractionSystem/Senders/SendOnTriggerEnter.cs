namespace ML.GameCommands
{
    using UnityEngine;

    public class SendOnTriggerEnter : TriggerCommand
    {
        public LayerMask layerMask;

        private void OnTriggerEnter(Collider other)
        {
            if (0 != (layerMask.value & 1 << other.gameObject.layer))
            {
                Send();
            }
        }
    }
}