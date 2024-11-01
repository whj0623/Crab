using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodedPlayer : MonoBehaviour
{
    public List<Rigidbody> bodyParts = new();
    public float explosionForce = 500f; // ���� ũ��
    public float explosionRadius = 5f; // ���� �ݰ�
    public float randomForce = 200f; // �߰��� �����ϰ� ���� ��
    private void Start()
    {
        foreach (Rigidbody part in bodyParts)
        {
            part.isKinematic = false;
            part.transform.SetParent(null); 
            Vector3 randomDir = Random.insideUnitSphere;
            part.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            part.AddForce(randomDir * randomForce); 
        }
    }
}
