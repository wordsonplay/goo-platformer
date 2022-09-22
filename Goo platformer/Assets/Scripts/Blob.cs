using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Blob : MonoBehaviour
{
    [SerializeField] private Rigidbody2D pointPrefab;
    [SerializeField] private int nPoints = 10;
    [SerializeField] private int connectEvery = 2;
    [SerializeField] private int radius = 2;
    [SerializeField] private float springForce = 1;

    new private Rigidbody2D rigidbody;
    private Rigidbody2D[] points;

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
        // connection to centre point
        for (int i = 0; i < points.Length; i++)
        {
            ApplySpringForce(rigidbody, points[i], radius);
        }        

        // connection to neighbours
        for (int n = 1; n <= connectEvery; n++) 
        {
            float dRest = 2 * radius * Mathf.Sin(n * Mathf.PI / nPoints);
            for (int i = 0; i < points.Length; i++)
            {
                int j = (i + n) % nPoints;
                ApplySpringForce(points[i], points[j], dRest);
            }        
        }

    }

    private void ApplySpringForce(Rigidbody2D rb1, Rigidbody2D rb2, float dRest) 
    {
        Vector3 v = rb1.transform.position - rb2.transform.position;
        float d = v.magnitude - dRest;
        v = v.normalized * d;
        rb1.AddForce(-v * springForce);
        rb2.AddForce(v * springForce);
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
