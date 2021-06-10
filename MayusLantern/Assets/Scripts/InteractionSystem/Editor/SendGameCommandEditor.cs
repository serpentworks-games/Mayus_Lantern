namespace ML.GameCommands
{
    using UnityEngine;
    using UnityEditor;

    [SelectionBase]
    [CustomEditor(typeof(SendGameCommand))]
    public class SendGameCommandEditor : Editor
    {

        private void OnSceneGUI()
        {
            var si = target as SendGameCommand;
            if (si.interactiveObject != null)
            {
                DrawInteraction(si);
            }
        }

        [DrawGizmo(GizmoType.NonSelected | GizmoType.Pickable | GizmoType.NotInSelectionHierarchy)]
        static void DrawConnectionGizmo(SendGameCommand sgc, GizmoType gizmoType)
        {
            if (sgc.interactiveObject != null)
            {
                var start = sgc.transform.position;
                var end = sgc.interactiveObject.transform.transform.position;
                if (end == start) end += sgc.interactiveObject.transform.forward * 1;
                var dir = (end - start).normalized;
                if (Application.isPlaying)
                {
                    Handles.color = Color.Lerp(Color.white, Color.green, sgc.TimeSinceLastSend);
                }
                else
                {
                    Handles.color = new Color(1, 1, 1, 0.25f);
                }
                Handles.DrawDottedLine(start, end, 5);
                Handles.ArrowHandleCap(0, start + (dir * 2), Quaternion.LookRotation(dir), 1, EventType.Repaint);
            }
        }

        public static void DrawInteraction(SendGameCommand command)
        {
            var start = command.transform.position;
            var end = command.interactiveObject.transform.position;
            var dir = (end - start).normalized;

            if (Application.isPlaying)
            {
                Handles.color = Color.Lerp(Color.white, Color.green, command.TimeSinceLastSend);
            }

            var steps = Mathf.FloorToInt((end - start).magnitude);

            for (var i = 0; i < steps; i++)
            {
                Handles.ArrowHandleCap(0, start + (dir * i), Quaternion.LookRotation(dir), 1, EventType.Repaint);
            }
        }
    }
}