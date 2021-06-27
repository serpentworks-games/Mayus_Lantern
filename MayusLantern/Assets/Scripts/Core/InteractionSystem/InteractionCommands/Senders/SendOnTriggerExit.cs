namespace ML.GameCommands
{
    using UnityEngine;

    public class SendOnTriggerExit : TriggerCommand
    {
        public LayerMask layerMask;

        private void OnTriggerExit(Collider other)
        {
            if (0 != (layerMask.value & 1 << other.gameObject.layer))
            {
                Send();
            }
        }
    }
}