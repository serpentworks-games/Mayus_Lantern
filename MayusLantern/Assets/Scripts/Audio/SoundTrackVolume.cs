namespace ML.Audio
{
    using UnityEngine;

    public class SoundTrackVolume : MonoBehaviour
    {

        public LayerMask layerMask;
        SoundTrack soundTrack;

        private void OnEnable()
        {
            soundTrack = GetComponentInParent<SoundTrack>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (0 != (layerMask.value & 1 << other.gameObject.layer))
            {
                soundTrack.PushTrack(this.name);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (0 != (layerMask.value & 1 << other.gameObject.layer))
            {
                soundTrack.PopTrack();
            }
        }
    }
}