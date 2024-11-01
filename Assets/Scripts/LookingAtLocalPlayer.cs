using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LookingAtLocalPlayer : MonoBehaviour
{
    public Transform localPlayerCam;
    public Text text;

    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        if (PhotonNetwork.LocalPlayer.IsLocal)
        {
            if (text != null)
                text.text = GetComponentInParent<PhotonView>().Owner.NickName;
            localPlayerCam = Camera.main?.gameObject.transform;
            if (localPlayerCam == null)
                StartCoroutine(FindLocalPlayerCam());
        }
    }

    private IEnumerator FindLocalPlayerCam()
    {
        while (localPlayerCam == null)
        {
            yield return null;
            localPlayerCam = Camera.main?.gameObject.transform;
        }
    }

    void FixedUpdate()
    {
        if (localPlayerCam != null)
        {
            Vector3 lookPos = localPlayerCam.position - transform.position;
            transform.rotation = Quaternion.LookRotation(lookPos);

            if (text != null)
                transform.rotation *= Quaternion.Euler(0, 180, 0);
        }

    }
}


