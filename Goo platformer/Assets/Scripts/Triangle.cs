using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle : MonoBehaviour
{
    [SerializeField] private Rigidbody2D p0;
    [SerializeField] private Rigidbody2D p1;
    [SerializeField] private Rigidbody2D p2;
    [SerializeField] private float desiredArea = 1;
    [SerializeField] private float springForce = 1;

    private Vector3 v0;
    private Vector3 v1;
    private Vector3 v2;

    void FixedUpdate()
    {
        float area = AreaOfTriangle();
        Debug.Log($"area = {area}");
        float delta = desiredArea - area;

        ApplyVolumeForce(p0, p1, p2, delta);
        ApplyVolumeForce(p1, p2, p0, delta);
        ApplyVolumeForce(p2, p0, p1, delta);
    }

    public float AreaOfTriangle()
    {
        Vector3 vi = p1.transform.position - p0.transform.position;
        Vector3 vj = p2.transform.position - p0.transform.position;
        Vector3 cross = Vector3.Cross(vi, vj);
        return cross.z;
    }

    private void ApplyVolumeForce(Rigidbody2D rb0, Rigidbody2D rb1, Rigidbody2D rb2, float dArea)
    {
        // apply a force to vertex p0, perpendicular to base P2-P1
        Vector3 b = rb2.transform.position - rb1.transform.position;
        Vector3 dir = Quaternion.AngleAxis(90, Vector3.forward) * b;
        rb0.AddForce(dir * dArea * springForce);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawLine(p0.transform.position, p0.transform.position + v0);
        Gizmos.DrawLine(p1.transform.position, p1.transform.position + v1);
        Gizmos.DrawLine(p2.transform.position, p2.transform.position + v2);

        Gizmos.color = (AreaOfTriangle() > 0 ? Color.green : Color.red);
        Gizmos.DrawLine(p0.transform.position, p1.transform.position);
        Gizmos.DrawLine(p1.transform.position, p2.transform.position);
        Gizmos.DrawLine(p2.transform.position, p0.transform.position);

    }
}
