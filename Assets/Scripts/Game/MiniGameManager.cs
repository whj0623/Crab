using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGameManager : MonoBehaviourPunCallbacks
{
    public virtual void GameStart()
    {

    }

    public virtual void GameOver()
    {

    }

    public virtual IEnumerator GameRoutine()
    {
        yield return null;

    }
}
