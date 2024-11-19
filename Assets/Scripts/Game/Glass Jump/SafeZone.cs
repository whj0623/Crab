using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HashTable = ExitGames.Client.Photon.Hashtable;

public class SafeZone : MonoBehaviour
{
    public GlassJumpManager glassJumpManager;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent<PhotonView>(out PhotonView view))
        {
            if (view != null  && !glassJumpManager.playersViewID.Contains(view.ViewID))
                glassJumpManager.playersViewID.Add(view.ViewID);
        }
    }
}
