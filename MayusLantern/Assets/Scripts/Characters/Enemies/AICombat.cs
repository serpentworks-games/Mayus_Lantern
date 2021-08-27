namespace ML.Characters.Enemies
{
    using UnityEngine;
    using ML.Core;

    public class AICombat : MonoBehaviour, IAction
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
        public bool isAlerted = false;
        public Transform targetLocation = null;

        //MeleeWeapon currentWeapon;
        [HideInInspector] public GameObject target;
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

        }

        public virtual void Update()
        {
            detectionRadius = targetScanner.detectionRadius;
            UpdateTimers();
            if (target == null) return;
            //TODO: Health Checks

            if (enemyCanAlertNeighbours)
            {
                if (canAlert)
                {
                    movement.Cancel();
                    AlertBehaviour();
                }
                else
                {
                    ClearAlertedNeighbours();
                }
            }

            if (enemyCanAttackMayu)
            {
                if (!GetIsInRange(target.transform))
                {
                    movement.MoveToDestination(target.transform.position);
                }
                else
                {
                    movement.Cancel();
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
            GetComponent<ActionScheduler>().StartAction(this);
            target = _target;
        }

        public virtual bool CanAttack(GameObject _target)
        {
            if (_target == null) return false;
            if (!movement.CanMoveTo(target.transform.position) && !GetIsInRange(target.transform)) return false;
            Transform targetToTest = _target.transform;
            return targetToTest != null;
        }

        public virtual void StopAttackAnim()
        {
            anim.ResetTrigger("StartAttack");
            anim.SetTrigger("StopAttack");
        }

        void StartAttackAnim()
        {
            anim.ResetTrigger("StopAttack");
            anim.SetTrigger("StartAttack");
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
            isAlerted = true;
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
            anim.SetTrigger("StartAlert");
        }

        void StopAlertAnim()
        {
            anim.ResetTrigger("StartAlert");
            anim.SetTrigger("StopAlert");
        }

        void AlertNeighbours()
        {
            foreach (AICombat neighbour in neighboursToAlert)
            {
                neighbour.target = target;
                neighbour.isAlerted = true;
                neighbour.targetLocation = target.transform;
            }
        }

        void ClearAlertedNeighbours()
        {
            foreach (AICombat neighbour in neighboursToAlert)
            {
                neighbour.Cancel();
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

        public virtual void Cancel()
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

        public virtual bool GetIsInRange(Transform targetTransform)
        {
            return Vector3.Distance(transform.position, targetTransform.position) < attackRange;
        }

        //Allows for runtime changing of the detection radius
        public virtual void SetDetectionRadius(float radius)
        {
            targetScanner.detectionRadius = radius;
        }

        public virtual float GetScannerRadius()
        {
            return targetScanner.detectionRadius;
        }

        //Anim Hit Event
        public virtual void Hit()
        {
            if (target == null) return;
            Debug.Log(transform.name + " is hitting " + target.name);
        }

        //Anim Alert Event
        public virtual void Alert()
        {
            if (target == null) return;
            Debug.Log(transform.name + " is alerting about " + target.name);
        }


        private void OnDrawGizmos()
        {
            targetScanner.EditorGizmo(transform);
        }
    }
}