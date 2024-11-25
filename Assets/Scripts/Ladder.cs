using Unity.VisualScripting;
using UnityEngine;

public class Ladder : MonoBehaviour
{
    private float climbSpeed = 5f;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
                player.onLadder = true; 
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
                player.onLadder = false;  
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null && player.onLadder)
            {
                float verticalInput = Input.GetAxis("Vertical");
                Vector3 climbDirection = new Vector3(0, verticalInput * climbSpeed * Time.deltaTime, 0);
                player.transform.position += climbDirection;
            }
        }
    }
}
