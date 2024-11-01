using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ladder : MonoBehaviour
{
    public float climbSpeed = 3f;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.TryGetComponent<CharacterController>(out CharacterController characterController) && other.gameObject.GetComponent<PhotonView>().IsMine)
        {
            if (characterController != null)
            {
                if (Input.GetKey(KeyCode.UpArrow))
                    characterController.Move(Vector3.up * climbSpeed * Time.deltaTime);
                else if (Input.GetKey(KeyCode.DownArrow))
                    characterController.Move(Vector3.down * climbSpeed * Time.deltaTime);
            }
        }
    }
}
