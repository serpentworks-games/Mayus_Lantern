namespace ML.Core.DamageSystem
{
    using UnityEngine;
    using ML.Characters.Player;

    [RequireComponent(typeof(Collider))]
    public class DeathVolume : MonoBehaviour
    {
        public new AudioSource audio;

        private void OnTriggerEnter(Collider other)
        {
            var pc = other.GetComponent<PlayerController>();

            if (pc != null)
            {
                pc.Die(new Damageable.DamageMessage());
            }

            if (audio != null)
            {
                audio.transform.position = other.transform.position;
                if (!audio.isPlaying) audio.Play();
            }
        }

        private void Reset()
        {
            if (LayerMask.LayerToName(gameObject.layer) == "Default")
            {
                gameObject.layer = LayerMask.NameToLayer("Environment");
            }

            var c = GetComponent<Collider>();

            if (c != null) c.isTrigger = true;
        }

    }
}