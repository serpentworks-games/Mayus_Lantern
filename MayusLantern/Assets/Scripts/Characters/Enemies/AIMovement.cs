namespace ML.Characters.Enemies
{
    using UnityEngine;
    using UnityEngine.AI;

    [RequireComponent(typeof(NavMeshAgent))]
    public class AIMovement : MonoBehaviour
    {
        public float maxMoveSpeed;
        [Range(0, 1)] public float speedFraction;

        NavMeshAgent agent;
        Animator anim;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            anim = GetComponent<Animator>();
        }

        private void Update()
        {
            UpdateAnimator();
        }

        public void MoveToDestination(Vector3 destination)
        {
            agent.destination = destination;
            agent.speed = maxMoveSpeed * Mathf.Clamp01(speedFraction);
            agent.isStopped = false;
        }

        public void StopMove()
        {
            agent.isStopped = true;
        }

        void UpdateAnimator()
        {
            Vector3 velocity = agent.velocity;
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);
            float speed = localVelocity.z;

            anim.SetFloat("ForwardSpeed", speed);
        }
    }
}