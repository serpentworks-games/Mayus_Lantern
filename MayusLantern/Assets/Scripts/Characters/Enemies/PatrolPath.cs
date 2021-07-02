using UnityEngine;

public class PatrolPath : MonoBehaviour
{
    public int GetNextIndex(int i)
    {
        if (i + 1 == transform.childCount)
        {
            return 0;
        }
        return i + 1;
    }

    public Vector3 GetWayPoint(int i)
    {
        return transform.GetChild(i).position;
    }

    public void EditorGizmo()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            int j = GetNextIndex(i);
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(GetWayPoint(i), 0.5f);
            Gizmos.DrawLine(GetWayPoint(i), GetWayPoint(j));
        }
    }

    private void OnDrawGizmos()
    {
        EditorGizmo();
    }
}