namespace ML.GameCommands
{
    using UnityEngine;

    public class SwitchMaterial : GameCommandHandler
    {
        [Tooltip("Target render to change")]
        public Renderer target;
        [Tooltip("The materials to switch between. Place the CURRENT material on the object first, then the material to be switched to second.")]
        public Material[] materials;

        int count;

        public override void PerformInteraction()
        {
            count++;
            target.material = materials[count % materials.Length];
        }

    }
}