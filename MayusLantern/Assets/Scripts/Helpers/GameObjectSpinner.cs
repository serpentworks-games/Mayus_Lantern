namespace ML.Helpers
{
    using UnityEngine;

    public class GameObjectSpinner : MonoBehaviour
    {
        public Vector3 axis = Vector3.up;
        public float speed = 1;

        private void Update()
        {
            transform.Rotate(axis, speed * Time.deltaTime);
        }
    }
}