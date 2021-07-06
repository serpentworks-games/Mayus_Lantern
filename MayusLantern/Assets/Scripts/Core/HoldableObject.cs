namespace ML.Core
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using ML.Characters.Player;

    public class HoldableObject : MonoBehaviour
    {
        [System.Serializable]
        public enum PhysicsType
        {
            GravityOnly, KinematicOnly, Both, Neither
        }

        [Tooltip("Does this use gravity, kinematics, both or neither?")]
        public PhysicsType physicsType;
        public bool isCarried;
        public Vector3 offset;
        public Collider meshCollider;
        public LayerMask layerMask;

        Rigidbody rb;
        Collider col;
        bool isInRange;
        PlayerController player;
        PlayerInput input;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();
        }

        private void Reset()
        {
            layerMask = LayerMask.NameToLayer("Everything");
            col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        private void Update()
        {
            if (!isCarried && isInRange)
            {
                if (input.InteractInput)
                {
                    PickUpObject();
                }
            }
            else if (isCarried)
            {
                if (input.InteractInput)
                {
                    DropObject();
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (0 != (layerMask.value & 1 << other.gameObject.layer))
            {
                player = other.GetComponent<PlayerController>();
                input = other.GetComponent<PlayerInput>();
                isInRange = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (0 != (layerMask.value & 1 << other.gameObject.layer))
            {
                player = null;
                input = null;
                isInRange = false;
            }
        }

        void PickUpObject()
        {
            Transform holdPoint = player.GetObjectHoldPoint();
            //anim: player.SetCarryMask(1);

            rb.useGravity = false;
            rb.isKinematic = true;
            transform.position = holdPoint.position + offset;
            transform.rotation = holdPoint.rotation;
            transform.parent = holdPoint;
            isCarried = true;
            col.enabled = false;
            meshCollider.enabled = false;
        }

        void DropObject()
        {
            Transform holdPoint = player.GetObjectHoldPoint();
            //anim: player.SetCarryMask(0);

            SetRigidbodyPhysics();
            transform.position = holdPoint.position + holdPoint.transform.forward;
            transform.rotation = holdPoint.rotation;
            transform.parent = null;
            isCarried = false;
            col.enabled = true;
            meshCollider.enabled = true;
        }

        void SetRigidbodyPhysics()
        {
            switch (physicsType)
            {
                case PhysicsType.GravityOnly:
                    rb.useGravity = true;
                    rb.isKinematic = false;
                    break;
                case PhysicsType.KinematicOnly:
                    rb.useGravity = false;
                    rb.isKinematic = true;
                    break;
                case PhysicsType.Both:
                    rb.useGravity = true;
                    rb.isKinematic = true;
                    break;
                case PhysicsType.Neither:
                    rb.useGravity = false;
                    rb.isKinematic = false;
                    break;
            }
        }
    }
}

