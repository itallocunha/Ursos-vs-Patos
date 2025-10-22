using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class EnemyPath2D : MonoBehaviour
{
    public List<Transform> waypoints = new List<Transform>();
    public float arriveRadius = 0.15f;

    public int Count => waypoints != null ? waypoints.Count : 0;

    public Vector2 GetWaypoint(int index)
    {
        if (waypoints == null || index < 0 || index >= waypoints.Count)
            return transform.position;

        var t = waypoints[index];
        return t ? (Vector2)t.position : (Vector2)transform.position;
    }
    
    void Awake()
    {
        AutoFillIfEmpty();
    }

    void OnValidate()
    {
        AutoFillIfEmpty();
    }

    void Reset()
    {
        AutoFillFromChildren();
    }

    private void AutoFillIfEmpty()
    {
        if (waypoints == null || waypoints.Count == 0)
            AutoFillFromChildren();
    }

    private void AutoFillFromChildren()
    {
        if (waypoints == null) waypoints = new List<Transform>();
        waypoints.Clear();
        foreach (Transform child in transform)
            waypoints.Add(child);
    }

    void OnDrawGizmos()
    {
        if (Count < 1) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < Count; i++)
        {
            var p = GetWaypoint(i);
            Gizmos.DrawWireSphere(p, arriveRadius);
            if (i < Count - 1)
            {
                var n = GetWaypoint(i + 1);
                Gizmos.DrawLine(p, n);
            }
        }
    }
}
