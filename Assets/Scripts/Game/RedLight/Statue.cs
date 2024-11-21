using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Statue : MonoBehaviour
{

    public IEnumerator Turn()
    {
        float duration = 0.5f; 
        float timeElapsed = 0f;

        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = Quaternion.Euler(0, 180, 0) * startRotation;

        while (timeElapsed < duration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, timeElapsed / duration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        transform.rotation = endRotation;
    }
}
