using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HashTable = ExitGames.Client.Photon.Hashtable;

public class SafeZone : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<PhotonView>(out PhotonView view))
        {
            if (view != null && view.IsMine)
            {
                HashTable props = new HashTable();
                props["survived"] = true;
                PhotonNetwork.LocalPlayer.SetCustomProperties(props);
            }
        }
    }
}
