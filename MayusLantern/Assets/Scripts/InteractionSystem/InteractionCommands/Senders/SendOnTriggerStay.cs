namespace ML.GameCommands
{
    using UnityEngine;

    public class SendOnTriggerStay : TriggerCommand
    {
        public LayerMask layerMask;

        private void OnTriggerStay(Collider other)
        {
            if (0 != (layerMask.value & 1 << other.gameObject.layer))
            {
                Send();
            }
        }
    }
}