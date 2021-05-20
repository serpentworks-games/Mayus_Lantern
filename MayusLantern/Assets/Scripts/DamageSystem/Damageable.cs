namespace ML.DamageSystem
{
    using UnityEngine;
    using ML.Core;
    using UnityEngine.Events;
    using System.Collections.Generic;
    using ML.Utils;

    public class Damageable : MonoBehaviour
    {

        //Information to pass when a Damage Message is sent
        //Helps to determine who and from where the attack came from
        //As well as if the actor should be stunned or is thrown
        public struct DamageMessage
        {
            public MonoBehaviour damager;
            public int amount;
            public Vector3 direction;
            public Vector3 damageSource;
            public bool throwing;
            public bool stunned;

            public bool stopCamera;
        }

        public int maxHitPoints;
        public float invunlerabilityTime;
        [Tooltip("Purely for editor GUI viz depending on the size of the Damageable Entity")]
        public float damageRadiusForGUIViz = 1.0f;

        [Range(0.0f, 360.0f)]
        public float hitAngle = 360.0f;
        [Range(0.0f, 360.0f)]
        public float hitForwardRotation = 360.0f;

        public bool isInvulnerable { get; set; }
        public bool isStunned { get; set; }
        public int currentHP { get; private set; }

        public UnityEvent OnDeath, OnReceiveDamage, OnHitWhileInvulnerable, OnBecomeVulnerable, OnResetDamage, OnStun;

        [EnforceType(typeof(IMessageReceiver))]
        public List<MonoBehaviour> onDamageMessageReceivers;

        float m_timeSinceLastHit = 0.0f;
        Collider m_Collider;

        System.Action schedule;

        private void Start()
        {
            ResetDamage();
            m_Collider = GetComponent<Collider>();
        }

        private void Update()
        {
            if (isInvulnerable)
            {
                m_timeSinceLastHit += Time.deltaTime;
                if (m_timeSinceLastHit > invunlerabilityTime)
                {
                    m_timeSinceLastHit = 0.0f;
                    isInvulnerable = false;
                    isStunned = false;
                    OnBecomeVulnerable.Invoke();
                }
            }
        }

        /// <summary>
        /// Resets values to default when damage is reset
        /// </summary>
        void ResetDamage()
        {
            currentHP = maxHitPoints;
            isInvulnerable = false;
            isStunned = false;
            m_timeSinceLastHit = 0.0f;
            OnResetDamage.Invoke();
        }

        /// <summary>
        /// Determines if the collider should be enabled or disabled when actor
        /// is damaged
        /// </summary>
        /// <param name="enabled">Whether the collider is active or not</param>
        public void SetColliderState(bool enabled)
        {
            m_Collider.enabled = enabled;
        }

        /// <summary>
        /// Applies damage to an actor, given the data passed through
        /// </summary>
        /// <param name="data">The DamageMessage info to be applied</param>
        public void ApplyDamage(DamageMessage data)
        {
            if (currentHP <= 0)
            {
                //If actor is already dead, ignore damage
                //Mostly applies to Mayu
                return;
            }
            if (isStunned)
            {
                //If actor is stunned, ignore damage to keep from extending the stun time
                return;
            }
            if (isInvulnerable)
            {
                //If actor is invulnerable, ignore damage
                OnHitWhileInvulnerable.Invoke();
                return;
            }

            Vector3 forward = transform.forward;
            forward = Quaternion.AngleAxis(hitForwardRotation, transform.up) * forward;

            //Project the direction the damage came from onto the plane formed by direction of the damage/stun
            Vector3 positionToDamager = data.damageSource - transform.position;
            positionToDamager -= transform.up * Vector3.Dot(transform.up, positionToDamager);

            //if the angle between the forward direction and the position of the damage is greater than half the hit angle, then the hit is outside the hitAngle.
            //Becomes important when enemies have 'invulnerable' areas
            if (Vector3.Angle(forward, positionToDamager) > hitAngle * 0.5f) return;

            //If the hit is within the angle, then start the invulnerable state, subtract damage from the HP (if applicable) and set the stunned state (if applicable)
            isInvulnerable = true;
            currentHP -= data.amount;
            isStunned = data.stunned;

            if (currentHP <= 0) schedule += OnDeath.Invoke; // Avoids a race condition when two objects kill each other
            else OnReceiveDamage.Invoke();

            if (isStunned) OnStun.Invoke();

            var messageTypeHP = currentHP <= 0 ? MessageType.DEAD : MessageType.DAMAGED;

            for (int i = 0; i < onDamageMessageReceivers.Count; i++)
            {
                var receiver = onDamageMessageReceivers[i] as IMessageReceiver;
                receiver.OnReceiveMessage(messageTypeHP, this, data);
            }
        }

        private void LateUpdate()
        {
            if (schedule != null)
            {
                schedule();
                schedule = null;
            }
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Vector3 forward = transform.forward;
            forward = Quaternion.AngleAxis(hitForwardRotation, transform.up) * forward;

            if (Event.current.type == EventType.Repaint)
            {
                UnityEditor.Handles.color = Color.blue;
                UnityEditor.Handles.ArrowHandleCap(0, transform.position, Quaternion.LookRotation(forward), 1.0f, EventType.Repaint);
            }

            UnityEditor.Handles.color = new Color(1.0f, 0.0f, 0.0f, 0.5f);
            forward = Quaternion.AngleAxis(-hitAngle * 0.5f, transform.up) * forward;
            UnityEditor.Handles.DrawSolidArc(transform.position, transform.up, forward, hitAngle, damageRadiusForGUIViz);
        }
#endif

    }
}