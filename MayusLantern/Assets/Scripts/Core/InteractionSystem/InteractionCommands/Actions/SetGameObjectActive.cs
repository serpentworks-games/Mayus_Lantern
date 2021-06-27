namespace ML.GameCommands
{
    using UnityEngine;

    public class SetGameObjectActive : GameCommandHandler
    {
        [Tooltip("The game object(s) to set active or inactive")]
        public GameObject[] targets;
        public bool isEnabled = true;

        public override void PerformInteraction()
        {
            foreach (var go in targets)
            {
                go.SetActive(isEnabled);
            }
        }
    }
}