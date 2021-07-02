namespace ML.Core.Weapons
{
    using UnityEngine;
    using System;
    using System.Collections.Generic;
    using ML.Audio;
    using ML.Core.DamageSystem;

    public class MeleeWeapon : MonoBehaviour
    {
        public int damage = 1;

        [System.Serializable]
        public class AttackPoint
        {
            public float radius;
            public Vector3 offset;
            public Transform attackRoot;

#if UNITY_EDITOR
            //Editor only in order to display the path of the attack
            [NonSerialized] public List<Vector3> previousPositions = new List<Vector3>();
#endif
        }

        public ParticleSystem hitParticlePrefab;
        public LayerMask targetLayerMask;

        public AttackPoint[] attackPoints = new AttackPoint[0];

        public TimeEffect[] effects;

        [Header("Audio")] public RandomAudioPlayer hitAudio, attackAudio;

        public bool isThrowingHit
        {
            get { return m_isThrowingHit; }
            set { m_isThrowingHit = value; }
        }

        public Color gizmoColor = new Color();

        protected GameObject m_Owner;

        protected Vector3[] m_previousPosition = null;
        protected Vector3 m_Direction;

        protected bool m_isThrowingHit = false;
        protected bool m_isInAttack = false;

        const int kParticleCount = 10;
        protected ParticleSystem[] m_particlePool = new ParticleSystem[kParticleCount];
        protected int m_currentParticleIndex = 0;

        protected static RaycastHit[] s_raycastHitCache = new RaycastHit[32];
        protected static Collider[] s_colliderCache = new Collider[32];

        void Awake()
        {
            if (hitParticlePrefab != null)
            {
                for (int i = 0; i < kParticleCount; i++)
                {
                    m_particlePool[i] = Instantiate(hitParticlePrefab);
                    m_particlePool[i].Stop();
                }
            }
        }

        private void OnEnable()
        {

        }

        /// <summary>
        /// Whoever owns the weapon is responsible for calling this. Setting the owner
        /// avoids 'self harm' from weapons when they collide with their owner
        /// </summary>
        /// <param name="owner">GameObject that 'owns' this weapon</param>
        public void SetOwner(GameObject owner)
        {
            m_Owner = owner;
        }

        public void BeginAttack(bool isThrownAttack)
        {
            if (attackAudio != null) attackAudio.PlayRandomClip();

            isThrowingHit = isThrownAttack;

            m_isInAttack = true;

            m_previousPosition = new Vector3[attackPoints.Length];

            for (int i = 0; i < attackPoints.Length; i++)
            {
                Vector3 worldPos = attackPoints[i].attackRoot.position +
                    attackPoints[i].attackRoot.TransformVector(attackPoints[i].offset);
                m_previousPosition[i] = worldPos;

#if UNITY_EDITOR
                attackPoints[i].previousPositions.Clear();
                attackPoints[i].previousPositions.Add(m_previousPosition[i]);
#endif
            }
        }

        public void EndAttack()
        {
            m_isInAttack = false;
#if UNITY_EDITOR
            for (int i = 0; i < attackPoints.Length; i++)
            {
                attackPoints[i].previousPositions.Clear();
            }
#endif
        }

        private void FixedUpdate()
        {
            if (m_isInAttack)
            {
                for (int i = 0; i < attackPoints.Length; i++)
                {
                    AttackPoint point = attackPoints[i];

                    Vector3 worldPos = point.attackRoot.position + point.attackRoot.TransformVector(point.offset);
                    Vector3 attackVector = worldPos - m_previousPosition[i];

                    if (attackVector.magnitude < 0.001f)
                    {
                        //A zero vector for the sphere cast won't receive any results, even with collider overlap on the cast
                        //So we set a very tiny magnitude to the forward cast to be sure it will catch anything
                        // overlapping the 'stationary' sphere cast
                        attackVector = Vector3.forward * 0.0001f;
                    }

                    Ray ray = new Ray(worldPos, attackVector.normalized);

                    int contacts = Physics.SphereCastNonAlloc(ray, point.radius,
                        s_raycastHitCache, attackVector.magnitude, ~0, QueryTriggerInteraction.Ignore);

                    for (int k = 0; k < contacts; k++)
                    {
                        Collider col = s_raycastHitCache[k].collider;

                        if (col != null) CheckDamage(col, point);
                    }

                    m_previousPosition[i] = worldPos;

#if UNITY_EDITOR
                    point.previousPositions.Add(m_previousPosition[i]);
#endif
                }
            }
        }

        private bool CheckDamage(Collider collider, AttackPoint point)
        {
            Damageable damageable = collider.GetComponent<Damageable>();

            //If the object has no damageable component, return false and end the attack
            if (damageable == null) return false;

            //If the object is the owner, return true
            //Alivates self harm, but without ending the attack (so we can't 
            //"bounce off of ourselves")
            if (damageable.gameObject == m_Owner) return true;

            //If it hits an object not in our target layer, 
            //end the attack and 'bounce' off of the object
            if ((targetLayerMask.value & (1 << collider.gameObject.layer)) == 0) return false;

            if (hitAudio != null)
            {
                //Grab the renderer off of the gameobject
                var renderer = collider.GetComponent<Renderer>();

                //If the gameobject does not have one, grab it from it's child
                if (!renderer) renderer = collider.GetComponentInChildren<Renderer>();

                //If it does have a renderer, play a random clip based on it
                if (renderer) hitAudio.PlayRandomClip(renderer.sharedMaterial);

                //If it doesn't have a renderer at all, play the default
                else hitAudio.PlayRandomClip();
            }

            Damageable.DamageMessage data = new Damageable.DamageMessage();

            data.amount = damage;
            data.damager = this;
            data.direction = m_Direction.normalized;
            data.damageSource = m_Owner.transform.position;
            data.throwing = m_isThrowingHit;
            data.stopCamera = false;

            damageable.ApplyDamage(data);

            if (hitParticlePrefab != null)
            {
                m_particlePool[m_currentParticleIndex].transform.position = point.attackRoot.transform.position;
                m_particlePool[m_currentParticleIndex].time = 0;
                m_particlePool[m_currentParticleIndex].Play();
                m_currentParticleIndex = (m_currentParticleIndex + 1) % kParticleCount;
            }

            return true;
        }

        private void OnDrawGizmosSelected()
        {
            for (int i = 0; i < attackPoints.Length; i++)
            {
                AttackPoint point = attackPoints[i];
                if (point.attackRoot != null)
                {
                    Vector3 worldPos = point.attackRoot.TransformVector(point.offset);
                    Gizmos.color = gizmoColor;
                    Gizmos.DrawSphere(point.attackRoot.position + worldPos, point.radius);
                }

                if (point.previousPositions.Count > 1)
                {
                    UnityEditor.Handles.DrawAAPolyLine(10, point.previousPositions.ToArray());
                }
            }
        }

    }
}