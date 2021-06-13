namespace ML.GameCommands
{
    using UnityEngine;
    public class PlayAnimationClip : GameCommandHandler
    {
        [Tooltip("The animation clip(s) to play with this action")]
        public Animation[] animationClips;

        public override void PerformInteraction()
        {
            foreach (var a in animationClips)
            {
                a.Play();
            }
        }
    }
}