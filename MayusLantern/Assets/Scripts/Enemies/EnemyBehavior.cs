namespace ML.Enemies {
    using UnityEngine;
    using ML.Core;

    [DefaultExecutionOrder(100)]
    public class EnemyBehavior : MonoBehaviour, IMessageReceiver {
        public TargetScanner playerScanner;

        public void OnReceiveMessage(MessageType type, object sender, object msg)
        {
            Debug.Log("Message Received!");
        }

        private void OnDrawGizmos() {
            playerScanner.EditorGizmo(transform);
        }
    }
}