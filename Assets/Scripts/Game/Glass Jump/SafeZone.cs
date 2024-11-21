using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HashTable = ExitGames.Client.Photon.Hashtable;

public class SafeZone : MonoBehaviour
{
    public MiniGameManager miniGameManager;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<PhotonView>(out PhotonView view))
        {
            if (view != null  && !miniGameManager.survivedPlayersViewID.Contains(view.ViewID))
                miniGameManager.survivedPlayersViewID.Add(view.ViewID);
        }
    }
}
