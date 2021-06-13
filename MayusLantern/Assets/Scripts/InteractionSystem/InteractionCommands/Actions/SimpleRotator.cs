using UnityEngine;

namespace ML.GameCommands
{
    public class SimpleRotator : SimpleTransformer
    {

        [Tooltip("Axis upon which to effect the rotation")]
        public Vector3 axis = Vector3.forward;
        [Tooltip("Start position of the rotation, in degrees")]
        [Range(0, 360)] public float start = 0;
        [Tooltip("End position of the rotation, in degrees")]
        [Range(0, 360)] public float end = 90;

        public override void PerformTransform(float position)
        {
            var curvePos = accelCurve.Evaluate(position);
            var q = Quaternion.AngleAxis(Mathf.Lerp(start, end, curvePos), axis);
            transform.localRotation = q;
        }
    }
}