using Hanzzz.MeshDemolisher;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Glass : MonoBehaviour
{
    public bool breakable = false;
    public Material material;
    private MeshDemolisher meshDemolisher = new();
    private List<GameObject> fragments = new();
    public List<Transform> points;
    private List<GameObject> breakPointObjects = new();

    private void OnCollisionEnter(Collision collision)
    {
        GameObject otherObject = collision.gameObject;
        Transform currentParent = otherObject.transform;
        while (currentParent.parent != null)
            currentParent = currentParent.parent;
        if (currentParent.CompareTag("Player"))
        {
            if (breakable)
            {
                points = GenerateRandomBreakPoints(gameObject, 40);
                fragments = meshDemolisher.Demolish(gameObject, points, material);

                foreach (GameObject fragment in fragments)
                {
                    fragment.AddComponent<Rigidbody>();
                    fragment.AddComponent<BoxCollider>();
                }

                foreach (GameObject point in breakPointObjects)
                    Destroy(point);

                Destroy(gameObject);
            }
        }
        else
            gameObject.GetComponent<Rigidbody>().isKinematic = true;
    }


    public List<Transform> GenerateRandomBreakPoints(GameObject target, int numPoints)
    {
        List<Transform> breakPoints = new List<Transform>();
        MeshFilter meshFilter = target.GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.mesh;

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        for (int i = 0; i < numPoints; i++)
        {
            int triIndex = Random.Range(0, triangles.Length / 3) * 3;
            Vector3 v0 = vertices[triangles[triIndex]];
            Vector3 v1 = vertices[triangles[triIndex + 1]];
            Vector3 v2 = vertices[triangles[triIndex + 2]];

            Vector3 randomPoint = GetRandomPointInTriangle(v0, v1, v2);
            GameObject pointGO = new GameObject("BreakPoint");
            pointGO.transform.position = target.transform.TransformPoint(randomPoint);
            breakPoints.Add(pointGO.transform);

            breakPointObjects.Add(pointGO);
        }

        return breakPoints;
    }

    private Vector3 GetRandomPointInTriangle(Vector3 v0, Vector3 v1, Vector3 v2)
    {
        float r1 = Random.value;
        float r2 = Random.value;

        Vector3 randomPoint = (1 - Mathf.Sqrt(r1)) * v0 + (Mathf.Sqrt(r1) * (1 - r2)) * v1 + (Mathf.Sqrt(r1) * r2) * v2;
        return randomPoint;
    }
}
