namespace ML.GameCommands
{
    using UnityEngine;

    public class ParticleSystemEmit : GameCommandHandler
    {
        [Tooltip("The particle system(s) to being emitting with this action")]
        public ParticleSystem[] particleSystems;
        [Tooltip("Number of particles to emit on the system")]
        public int count;

        public override void PerformInteraction()
        {
            foreach (var ps in particleSystems)
            {
                ps.Emit(count);
            }
        }
    }
}