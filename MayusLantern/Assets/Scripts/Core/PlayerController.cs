namespace ML.Core
{
    using UnityEngine;

    public class PlayerController : MonoBehaviour
    {
        public static PlayerController instance { get; internal set; }
        public object CameraSettings { get; internal set; }
        public bool respawning { get; internal set; }
    }
}