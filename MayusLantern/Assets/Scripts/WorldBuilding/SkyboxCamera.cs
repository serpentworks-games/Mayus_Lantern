namespace ML.WorldBuilding
{
    using UnityEngine;

    [RequireComponent(typeof(Camera))]
    public class SkyboxCamera : MonoBehaviour
    {
        [Tooltip("Main camera in the sceen. If null, it defaults to Camera.main")]
        new public Camera camera;
        [Tooltip("A smaller value increases the scale of the skybox")]
        public float movementCoefficient = 0.01f;

        Camera skyCam;
        Transform camTransform;

        private void Start()
        {
            camera.clearFlags = CameraClearFlags.Depth;
            camTransform = camera.transform;
            skyCam = GetComponent<Camera>();
        }

        private void OnPreRender()
        {
            if (camera != null)
            {
                skyCam.fieldOfView = camera.fieldOfView;
                transform.rotation = camTransform.rotation;
                transform.localPosition = camTransform.position * movementCoefficient;
            }
        }
    }
}