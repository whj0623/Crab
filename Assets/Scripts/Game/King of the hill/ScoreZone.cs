using Photon.Pun;
using UnityEngine;

public class ScoreZone : MonoBehaviour
{
    public KingOfTheHillManager manager;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PhotonView photonView = other.gameObject.GetComponent<PhotonView>();
            if (photonView != null && photonView.IsMine)
                manager.UpdateScore(photonView.OwnerActorNr, Time.deltaTime);
        }
    }
}
