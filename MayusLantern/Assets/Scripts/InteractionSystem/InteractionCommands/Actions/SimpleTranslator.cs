namespace ML.GameCommands
{
    using UnityEngine;

    public class SimpleTranslator : SimpleTransformer
    {

        [Tooltip("Rigidbody to move via this translator action")]
        public new Rigidbody rigidbody;
        public Vector3 start = -Vector3.forward;
        public Vector3 end = Vector3.forward;

        public override void PerformTransform(float position)
        {
            var curvePos = accelCurve.Evaluate(position);
            var pos = transform.TransformPoint(Vector3.Lerp(start, end, curvePos));

            Vector3 deltaPos = pos - rigidbody.position;

            if (Application.isEditor && !Application.isPlaying)
            {
                rigidbody.transform.position = pos;
            }

            rigidbody.MovePosition(pos);

            if (m_Platform != null)
            {
                m_Platform.MoveCharacterController(deltaPos);
            }
        }
    }
}