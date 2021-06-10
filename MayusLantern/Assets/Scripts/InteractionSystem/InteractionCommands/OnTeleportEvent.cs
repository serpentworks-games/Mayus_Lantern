namespace ML.GameCommands
{
    using UnityEngine;

    /// <summary>
    /// A dummy class to determine what needs to be teleported by teleport events
    /// </summary>
    public class OnTeleportEvent : MonoBehaviour
    {
        public virtual void OnTeleport(Teleporter teleporter)
        {

        }
    }
}