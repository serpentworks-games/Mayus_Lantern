using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ML.Utils
{
    //A simple class to editor visualation to colliders
    public class VisualTriggerVolume : MonoBehaviour
    {
        [SerializeField] Color color;

        private void OnDrawGizmos()
        {
            Matrix4x4 rotMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.matrix = rotMatrix;

            Gizmos.color = color;

            if (GetComponent<BoxCollider>())
                Gizmos.DrawCube(GetComponent<BoxCollider>().center, GetComponent<BoxCollider>().size);
            else if (GetComponent<SphereCollider>())
                Gizmos.DrawSphere(GetComponent<SphereCollider>().center, GetComponent<SphereCollider>().radius);
            else
                return;
        }
    }
}