using UnityEngine;
using Photon.Pun;
using System.Collections;

public class PlayerVisibility : MonoBehaviourPunCallbacks
{
    public Camera playerCamera;
    public CameraShake cs;
    void Start()
    {  
        SetupCamera();
    }

    public void SetupCamera()
    {
        if (photonView.IsMine)
        {
            playerCamera.gameObject.SetActive(true);
            SetLayer(gameObject, LayerMask.NameToLayer("IgnoreForSelf"));
            playerCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("IgnoreForSelf"));
        }
        else
            playerCamera.gameObject.SetActive(false);
    }
    public void DeadCamera()
    {
        SetLayer(gameObject,LayerMask.NameToLayer("Default"));
    }

    void SetLayer(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
            SetLayer(child.gameObject, newLayer);
    }
    
}
