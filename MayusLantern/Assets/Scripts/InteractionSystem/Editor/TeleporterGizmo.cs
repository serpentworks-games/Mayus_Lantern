namespace ML.GameCommands
{
    using UnityEditor;
    using UnityEngine;

    public static class CommandGizmos
    {
        static GUIStyle sceneStyle;

        static CommandGizmos()
        {
            sceneStyle = new GUIStyle("button");
            sceneStyle.fontStyle = FontStyle.Bold;
            sceneStyle.normal.textColor = Color.white;
            sceneStyle.margin = sceneStyle.overflow = sceneStyle.padding = new RectOffset(3, 3, 3, 3);
            sceneStyle.richText = true;
            sceneStyle.alignment = TextAnchor.MiddleLeft;
        }

        static void DrawStyle(Vector3 position, string text, string warning = "", float distance = 10)
        {
            if (!string.IsNullOrEmpty(warning))
            {
                text = text + "<color=red>" + warning + "</color>";
            }

            if ((Camera.current.transform.position - position).magnitude <= distance)
            {
                Handles.Label(position, text, sceneStyle);
            }
        }

        [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.NotInSelectionHierarchy, typeof(Teleporter))]
        static void DrawTeleporterGizmos(Teleporter teleporter, GizmoType gizmoType)
        {
            if (teleporter.destinationTransform)
            {
                DrawStyle(teleporter.transform.position, "Teleporter Enterance");
                Handles.color = Color.yellow * 0.5f;
                Handles.DrawDottedLine(teleporter.transform.position, teleporter.destinationTransform.position, 5);
                DrawStyle(teleporter.destinationTransform.position, "Teleporter Exit");
            }
            else
            {
                DrawStyle(teleporter.transform.position, "Teleporter Enterence", "WARNING: No Destination Present!");
            }
        }
    }
}