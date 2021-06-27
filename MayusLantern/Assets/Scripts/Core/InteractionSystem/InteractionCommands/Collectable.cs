namespace ML.GameCommands
{
    using UnityEngine;

    [RequireComponent(typeof(Collider))]
    public class Collectable : MonoBehaviour
    {

        new public Collider collider;
        [Tooltip("What layers can interact with this collectable?")]
        public LayerMask layerMask;
        [Tooltip("Particle effect to trigger when this is collected")]
        public GameObject collectEffect;
        [Tooltip("Audio to play when collected")]
        public AudioClip onCollectAudio;
        [Tooltip("Does this collectable 'disappear' when collected?")]
        public bool disableOnCollect = false;

        private void Reset()
        {
            collider = GetComponent<Collider>();
            collider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (CanCollect(other))
                Collect(other);
        }

        protected virtual void Collect(Collider other)
        {
            if (collectEffect) collectEffect.SetActive(true);

            if (onCollectAudio)
            {

                var audio = GetComponent<AudioSource>();

                if (audio) audio.PlayOneShot(onCollectAudio);
            }

            var collector = other.GetComponent<Collector>();

            if (collector) collector.OnCollect(this);

            if (disableOnCollect) gameObject.SetActive(false);
        }

        bool CanCollect(Collider other)
        {
            return 0 != (layerMask.value & 1 << other.gameObject.layer);
        }
    }
}