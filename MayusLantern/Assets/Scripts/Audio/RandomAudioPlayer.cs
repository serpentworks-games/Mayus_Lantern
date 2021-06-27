namespace ML.Audio
{
    using UnityEngine;
    using System;
    using System.Collections.Generic;
    using Random = UnityEngine.Random;

    [RequireComponent(typeof(AudioSource))]
    public class RandomAudioPlayer : MonoBehaviour
    {
        [Serializable]
        public class MaterialAudioOverride
        {
            public Material[] materials;
            public SoundBank[] banks;
        }

        [Serializable]
        public class SoundBank
        {
            public string bankName;
            public AudioClip[] clips;
        }

        public bool randomizePitch = true;
        public float pitchRandomRange = 0.2f;
        public float playDelay = 0;
        public SoundBank defaultSoundBank = new SoundBank();
        public MaterialAudioOverride[] overrides;

        //TODO: hide these once testing is done
        public bool playing;
        public bool canPlay;

        public AudioSource audioSource { get { return m_audioSource; } }
        public AudioClip clip { get; private set; }

        protected AudioSource m_audioSource;
        protected Dictionary<Material, SoundBank[]> m_Lookup = new Dictionary<Material, SoundBank[]>();

        private void Awake()
        {
            m_audioSource = GetComponent<AudioSource>();
            for (int i = 0; i < overrides.Length; i++)
            {
                foreach (var material in overrides[i].materials)
                {
                    m_Lookup[material] = overrides[i].banks;
                }
            }
        }

        /// <summary>
        ///  Will pick a random clip to play in the assigned bank. If a material is passed
        ///  instead, it will pick the override for that material, if found. If not, it plays
        ///  the defaul clip.
        /// </summary>
        /// <param name="overrideMaterial"></param>
        /// <returns>Returns the chosen audio clip, null if none</returns>
        public AudioClip PlayRandomClip(Material overrideMaterial, int bankID = 0)
        {
            if (overrideMaterial == null) return null;
            return InternalPlayRandomClip(overrideMaterial, bankID);
        }

        /// <summary>
        /// Picks a random clip to playi n the assigned list
        /// </summary>
        public void PlayRandomClip()
        {
            clip = InternalPlayRandomClip(null, bankID: 0);
        }

        /// <summary>
        /// Handles returning the audio clip to play internally in the class
        /// </summary>
        /// <param name="overrideMaterial"></param>>
        AudioClip InternalPlayRandomClip(Material overrideMaterial, int bankID)
        {
            SoundBank[] banks = null;
            var bank = defaultSoundBank;
            if (overrideMaterial != null)
            {
                if (m_Lookup.TryGetValue(overrideMaterial, out banks))
                {
                    if (bankID < banks.Length)
                    {
                        bank = banks[bankID];
                    }
                }
            }
            if (bank.clips == null || bank.clips.Length == 0) return null;

            var clip = bank.clips[Random.Range(0, bank.clips.Length)];
            if (clip == null) return null;

            m_audioSource.pitch = randomizePitch ? Random.Range(1.0f - pitchRandomRange, 1.0f + pitchRandomRange) : 1.0f;
            m_audioSource.clip = clip;
            m_audioSource.PlayDelayed(playDelay);

            return clip;
        }

    }
}