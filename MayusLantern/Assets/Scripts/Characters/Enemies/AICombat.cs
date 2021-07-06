namespace ML.Characters.Enemies
{
    using UnityEngine;

    public class AICombat : MonoBehaviour
    {
        [Header("Can this enemy attack Mayu, alert its neighbours, or do both?")]
        public bool enemyCanAttackMayu = true;
        public bool enemyCanAlertNeighbours = false;

        [Header("Attack Params")]
        public Transform weaponTransform = null;
        //public MeleeWeapon defaultWeapon = null; 
        public float attackSpeed = 2f;
        public float attackRange = 5f;
        public TargetScanner targetScanner;

        [Header("Alert Params")]
        public float timeToStopAlert = 5f;
        public AICombat[] neighboursToAlert = null;
        public float detectionRadius;
        public bool canAlert = false;

        //MeleeWeapon currentWeapon;
        GameObject target;
        float timeSinceLastAttack = Mathf.Infinity;
        float timeSinceLastAlert = Mathf.Infinity;
        AIMovement movement;
        Animator anim;

        private void Awake()
        {
            anim = GetComponent<Animator>();
            movement = GetComponent<AIMovement>();
        }

        private void Start()
        {
            //EquipWeapon(defaultWeapon);
            detectionRadius = targetScanner.detectionRadius;
        }

        public virtual void Update()
        {
            UpdateTimers();
            if (target == null) return;
            //TODO: Health Checks

            if (enemyCanAlertNeighbours)
            {
                if (canAlert)
                {
                    movement.StopMove();
                    AlertBehaviour();
                }
                else
                {
                    ClearAlertedNeighbours();
                }
            }

            if (enemyCanAttackMayu)
            {
                if (!GetIsInRange())
                {
                    movement.MoveToDestination(target.transform.position);
                }
                else
                {
                    movement.StopMove();
                    AttackBehaviour();
                }
            }
        }

        public virtual void AttackBehaviour()
        {
            transform.LookAt(target.transform.position);
            if (timeSinceLastAttack > attackSpeed)
            {
                StartAttackAnim();
                timeSinceLastAttack = 0f;
            }
        }

        public virtual void Attack(GameObject _target)
        {
            target = _target;
        }

        public virtual bool CanAttack(GameObject _target)
        {
            if (_target == null) return false;
            Transform targetToTest = _target.transform;
            return targetToTest != null;
        }

        public virtual void StopAttackAnim()
        {
            anim.ResetTrigger("AttackPlayer");
            anim.SetTrigger("StopAttack");
        }

        void StartAttackAnim()
        {
            anim.ResetTrigger("StopAttack");
            anim.SetTrigger("AttackPlayer");
        }
        public virtual void AlertBehaviour()
        {
            if (timeSinceLastAlert > timeToStopAlert)
            {
                if (neighboursToAlert != null)
                {
                    AlertNeighbours();
                }
                else
                {
                    Debug.LogError(transform.name + " does not have any neighbours assigned in the inspector!");
                }

                StartAlertAnim();
                timeSinceLastAlert = 0f;
            }
            else
            {
                canAlert = false;
            }
        }

        public virtual void Alert(GameObject player)
        {
            target = player;
        }

        public virtual bool CanAlert(GameObject _target)
        {
            if (_target == null) return false;
            Transform targetToTest = _target.transform;
            return targetToTest != null;
        }

        void StartAlertAnim()
        {
            anim.ResetTrigger("StopAlert");
            anim.SetTrigger("AlertNeighbours");
        }

        void StopAlertAnim()
        {
            anim.ResetTrigger("AlertNeighbours");
            anim.SetTrigger("StopAlert");
        }

        void AlertNeighbours()
        {
            foreach (AICombat neighbour in neighboursToAlert)
            {
                neighbour.target = target;
            }
        }

        void ClearAlertedNeighbours()
        {
            foreach (AICombat neighbour in neighboursToAlert)
            {
                neighbour.CancelAction();
            }
        }

        private void UpdateTimers()
        {
            timeSinceLastAttack += Time.deltaTime;
            timeSinceLastAlert += Time.deltaTime;
        }

        public virtual GameObject FindTarget(GameObject player, Transform detector, bool useHeightDifference)
        {
            return targetScanner.Detect(player, detector, useHeightDifference);
        }

        public virtual void CancelAction()
        {
            if (enemyCanAlertNeighbours)
            {
                StopAlertAnim();
            }
            if (enemyCanAttackMayu)
            {
                StopAttackAnim();
            }

            target = null;
        }

        //Equip Weapon

        bool GetIsInRange()
        {
            return Vector3.Distance(transform.position, target.transform.position) < attackRange;
        }


        //Anim Hit Event
        public virtual void Hit()
        {
            if (target == null) return;
            Debug.Log(transform.name + " is hitting " + target.name);
        }

        public virtual void Alert()
        {

        }


        private void OnDrawGizmos()
        {
            targetScanner.EditorGizmo(transform);
        }
    }
}