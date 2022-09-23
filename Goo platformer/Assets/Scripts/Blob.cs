using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(Rigidbody2D))]
public class Blob : MonoBehaviour
{
    [SerializeField] private Rigidbody2D pointPrefab;
    [SerializeField] private int nPoints = 40;
    [SerializeField] private int connectEvery = 2;
    [SerializeField] private int radius = 2;
    [SerializeField] private float centreSpringForce = 10;
    [SerializeField] private float surfaceSpringForce = 50;
    [SerializeField] private float volumeSpringForce = 50;

    new private Rigidbody2D rigidbody;
    private Rigidbody2D[] points;

    public int Size {
        get { return nPoints; }
    }

    public Vector3 this[int i]
    {
        get { return points[i].transform.position; }
    }

    void Start()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        points = new Rigidbody2D[nPoints];
        
        for (int i = 0; i < nPoints; i++) 
        {
            points[i] = Instantiate(pointPrefab);
            points[i].transform.parent = transform;

            float angle = 360f * i / nPoints;
            Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
            points[i].transform.localPosition = rot * Vector3.right * radius;

            points[i].transform.parent = null;
        }

    }

    void FixedUpdate()
    {
        ApplyCentreForces();
        ApplySurfaceForces();
        ApplyVolumeForces();

    }

    private void ApplyCentreForces()
    {
        // connection to centre point
        for (int i = 0; i < points.Length; i++)
        {
            ApplySpringForce(rigidbody, points[i], radius, centreSpringForce);
        }        
    }

    private void ApplySurfaceForces()
    {
        // connection to neighbours
        for (int n = 1; n <= connectEvery; n++) 
        {
            float dRest = 2 * radius * Mathf.Sin(n * Mathf.PI / nPoints);
            for (int i = 0; i < points.Length; i++)
            {
                int j = (i + n) % nPoints;
                ApplySpringForce(points[i], points[j], dRest, surfaceSpringForce);
            }        
        }

    }

    private void ApplySpringForce(Rigidbody2D rb1, Rigidbody2D rb2, float dRest, float spring) 
    {
        Vector3 v = rb1.transform.position - rb2.transform.position;
        float d = v.magnitude - dRest;
        v = v.normalized * d;
        rb1.AddForce(-v * spring);
        rb2.AddForce(v * spring);
    }


    private void ApplyVolumeForces()
    {
        // calculate twice the area to avoid factor of 1/2
        float aRest = radius * radius * Mathf.Sin(2 * Mathf.PI / nPoints);
        for (int i = 0; i < points.Length; i++)
        {
            float a = AreaOfTriangle(i);
            float delta = aRest - a;
            
            int j = (i+1) % nPoints;
            ApplyVolumeForce(rigidbody, points[i], points[j], delta);
            ApplyVolumeForce(points[i], points[j], rigidbody, delta);
            ApplyVolumeForce(points[j], rigidbody, points[i], delta);
        }
    }

    private void ApplyVolumeForce(Rigidbody2D rb0, Rigidbody2D rb1, Rigidbody2D rb2, float dArea)
    {
        // apply a force to vertex p0, perpendicular to base P2-P1
        Vector3 b = rb2.transform.position - rb1.transform.position;
        Vector3 dir = Quaternion.AngleAxis(90, Vector3.forward) * b;
        rb0.AddForce(dir * dArea * volumeSpringForce);
    }


    public float AreaOfTriangle(int i)
    {
        int j = (i+1) % nPoints;
        Vector3 vi = points[i].transform.position - transform.position;
        Vector3 vj = points[j].transform.position - transform.position;
        Vector3 cross = Vector3.Cross(vi, vj);
        return cross.z;
    }


    void OnDrawGizmos() 
    {
        if (points == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.1f);

        Gizmos.color = Color.green;
        for (int i = 0; i < points.Length; i++) 
        {
            Gizmos.DrawWireSphere(points[i].transform.position, 0.1f);
        }

        // centre force
        for (int i = 0; i < points.Length; i++) 
        {
            Vector3 v = points[i].transform.position - transform.position;
            Gizmos.color = (v.magnitude > radius ? Color.red : Color.green);
            Gizmos.DrawLine(points[i].transform.position, transform.position);
        }

        // surface force
        for (int n = 1; n <= connectEvery; n++) 
        {
            float dRest = 2 * radius * Mathf.Sin(n * Mathf.PI / nPoints);
            for (int i = 0; i < points.Length; i++)
            {
                int j = (i + n) % nPoints;
                Vector3 v = points[i].transform.position - points[j].transform.position;
                Gizmos.color = (v.magnitude > dRest ? Color.red : Color.green);
                Gizmos.DrawLine(points[i].transform.position, points[j].transform.position);
            }        
        }


    }
}

[CustomEditor(typeof(Blob))]
public class BlobEditor : Editor
{
    public void OnSceneGUI()
    {
        if (!Application.IsPlaying(target))
        {
            return;
        }
        Blob blob = target as Blob;

        Vector3[] triangle = new Vector3[3];
        triangle[0] = blob.transform.position;

        for (int i = 0; i < blob.Size; i++) 
        {
            int j = (i + 1) % blob.Size;
            triangle[1] = blob[i];
            triangle[2] = blob[j];
            Handles.color = (blob.AreaOfTriangle(i) > 0 ? Color.green : Color.red);
            Handles.DrawAAConvexPolygon(triangle);
        }

    }
}