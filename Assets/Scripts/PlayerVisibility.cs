using UnityEngine;
using Photon.Pun;
using System.Collections;

public class PlayerVisibility : MonoBehaviourPunCallbacks
{
    public Camera playerCamera;

    void Start()
    {  
        SetupCamera();
    }

    public void SetupCamera()
    {
        if (photonView.IsMine)
        {
            playerCamera.gameObject.SetActive(true);
            SetLayerRecursively(gameObject, LayerMask.NameToLayer("IgnoreForSelf"));
            playerCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("IgnoreForSelf"));
        }
        else
            playerCamera.gameObject.SetActive(false);
    }
    public void DeadCamera()
    {
        SetLayerRecursively(gameObject, LayerMask.NameToLayer("Default"));
        //playerCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("IgnoreForSelf"));
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, newLayer);
    }
    
}
