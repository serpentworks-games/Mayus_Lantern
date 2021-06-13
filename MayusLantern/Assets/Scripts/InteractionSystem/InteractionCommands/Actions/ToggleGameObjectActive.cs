namespace ML.GameCommands
{
    using UnityEngine;

    public class ToggleGameObjectActive : GameCommandHandler
    {
        [Tooltip("The game object(s) to toggle active or inactive on")]
        public GameObject[] targets;

        public override void PerformInteraction()
        {
            foreach (var go in targets)
            {
                go.SetActive(!go.activeSelf);
            }
        }
    }
}