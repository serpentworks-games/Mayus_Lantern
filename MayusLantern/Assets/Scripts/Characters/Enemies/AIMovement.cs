namespace ML.Characters.Enemies
{
    using UnityEngine;
    using UnityEngine.AI;
    using ML.Core;

    [RequireComponent(typeof(NavMeshAgent))]
    public class AIMovement : MonoBehaviour, IAction
    {
        public float maxMoveSpeed;
        [Range(0, 1)] public float speedFraction;

        NavMeshAgent agent;
        Animator anim;
        ActionScheduler actionScheduler;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            anim = GetComponent<Animator>();
            actionScheduler = GetComponent<ActionScheduler>();
        }

        private void Update()
        {
            UpdateAnimator();
        }

        public void StartMoveAction(Vector3 destination)
        {
            actionScheduler.StartAction(this);
            MoveToDestination(destination);
        }

        public void MoveToDestination(Vector3 destination)
        {
            agent.destination = destination;
            agent.speed = maxMoveSpeed * Mathf.Clamp01(speedFraction);
            agent.isStopped = false;
        }

        public bool CanMoveTo(Vector3 destination)
        {
            NavMeshPath path = new NavMeshPath();
            return false;
        }

        public void Cancel()
        {
            agent.isStopped = true;
        }

        float GetPathLength(NavMeshPath path)
        {
            float total = 0;
            if (path.corners.Length < 2) return total;

            for (int i = 0; i < path.corners.Length - 1; i++)
            {
                total += Vector3.Distance(path.corners[i], path.corners[i + 1]);
            }

            return total;
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