using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Minigames
{
    GlassJump,
    KingOfTheHIll,
    RedLight,
    HideAndSeek
}
public class MiniGameManager : MonoBehaviourPunCallbacks
{
     public List<int> survivedPlayersViewID;
    
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
