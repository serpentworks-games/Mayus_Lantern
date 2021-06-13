namespace ML.GameCommands
{
    using UnityEngine;
    
    public class PlaySoundClip : GameCommandHandler {
        [Tooltip("The audio clip(s) to play with this action")]
        public AudioSource[] audioClips;

        public override void PerformInteraction(){
            foreach (var clip in audioClips)
            {
                clip.Play();
            }
        }
    }
}