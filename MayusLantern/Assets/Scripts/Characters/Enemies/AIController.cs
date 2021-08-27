namespace ML.Characters.Enemies
{
    using UnityEngine;

    public class AIController : MonoBehaviour
    {

        [System.Serializable]
        public class PatrollingInfo
        {
            public float suspicionTime = 3f;
            public float waypointDwellTime = 2f;
            public float waypointTolerance = 1f;
            public PatrolPath patrolPath = null;
        }

        [System.Serializable]
        public class AggressionInfo{
            public float aggroDurationTime = 5f;
            public float neighbourAggroDistance = 5f;
        }

        public float stunTime = 10f;
        public PatrollingInfo patrolInfo = new PatrollingInfo();
        public AggressionInfo aggroInfo = new AggressionInfo();

        Vector3 guardPosition;
        int currentWayPointIndex = 0;
        float timeSinceArrivedAtWayPoint = Mathf.Infinity;

        [HideInInspector] public float timeSinceLastSawPlayer = Mathf.Infinity;
        [HideInInspector] public float timeSinceLastStun = Mathf.Infinity;
        [HideInInspector] public float timeSinceLastAlert = Mathf.Infinity;
        [HideInInspector] public float timeSinceLastAggro = Mathf.Infinity;

        [HideInInspector] public GameObject player;
        [HideInInspector] public AIMovement movement;
        [HideInInspector] public AICombat combat;

        public virtual void Awake()
        {
            player = GameObject.FindGameObjectWithTag("Player");
            movement = GetComponent<AIMovement>();
            combat = GetComponent<AICombat>();
        }

        private void Start()
        {
            guardPosition = transform.position;
        }

        public virtual void UpdateTimers()
        {
            timeSinceArrivedAtWayPoint += Time.deltaTime;
            timeSinceLastSawPlayer += Time.deltaTime;
            timeSinceLastStun += Time.deltaTime;
            timeSinceLastAlert += Time.deltaTime;
        }

        public virtual void PatrolBehaviour()
        {
            Vector3 nextPosition = guardPosition;
            if (patrolInfo.patrolPath != null)
            {
                if (AtWayPoint())
                {
                    timeSinceArrivedAtWayPoint = 0f;
                    AdvanceWayPoints();
                }
                nextPosition = GetCurrentWayPoint();
            }

            if (timeSinceArrivedAtWayPoint > patrolInfo.waypointDwellTime)
            {
                movement.StartMoveAction(nextPosition);
            }
        }

        bool AtWayPoint()
        {
            float distanceToWayPoint = Vector3.Distance(transform.position, GetCurrentWayPoint());
            return distanceToWayPoint < patrolInfo.waypointTolerance;
        }

        Vector3 GetCurrentWayPoint()
        {
            return patrolInfo.patrolPath.GetWayPoint(currentWayPointIndex);
        }

        void AdvanceWayPoints()
        {
            currentWayPointIndex = patrolInfo.patrolPath.GetNextIndex(currentWayPointIndex);
        }

        public virtual void SuspicionBehaviour()
        {
            movement.Cancel();
        }

        public virtual void AttackBehaviour()
        {
            timeSinceLastSawPlayer = 0f;
            combat.Attack(player);

            AggroNearbyEnemies();
        }

        public virtual void StunnedBehaviour()
        {
            timeSinceLastStun = 0f;
            //Stun the beasty
        }

        public virtual void AlertBehaviour()
        {
            timeSinceLastSawPlayer = 0;
            combat.canAlert = true;
            combat.Alert(player);
        }

        public virtual bool CanSeePlayer()
        {
            return combat.FindTarget(player, transform, player == null);
        }

        public virtual void GenerateAggro(GameObject target){
            player = target;
            timeSinceLastAggro = 0;
        }

        public virtual void AggroNearbyEnemies(){
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, aggroInfo.neighbourAggroDistance, Vector3.up, 0);
            foreach (RaycastHit hit in hits)
            {
                AICombat neighbour = hit.transform.GetComponent<AICombat>();
                if(neighbour == null) continue;
                GenerateAggro(player);
            }
        }

        public virtual bool HasAggro(){
            float distToPlayer = Vector3.Distance(player.transform.position, transform.position);
            return distToPlayer < combat.GetScannerRadius() || timeSinceLastAggro < aggroInfo.aggroDurationTime;
        }

        private void OnDrawGizmos()
        {
            //Patrol Path Visual
            if (patrolInfo.patrolPath == null) return;
            patrolInfo.patrolPath.EditorGizmo();

            //Neighbor Agroo Range Visual
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, aggroInfo.neighbourAggroDistance);
        }
    }
}