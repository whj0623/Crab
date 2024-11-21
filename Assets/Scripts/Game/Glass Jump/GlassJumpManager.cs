using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GlassJumpManager : MiniGameManager
{
    public SafeZone safeZone;
    public GameManager gameManager;
    public GameOverUI gameOverUI;
   

    void Start()
    {
        survivedPlayersViewID = new List<int>();
    }
    public override void GameStart()
    {
        foreach (PlayerController player in gameManager.players)
            player.isFreeze = false;
    }

    [PunRPC]
    void EliminatePlayer(int viewID)
    {
        var player = PhotonView.Find(viewID).GetComponent<PlayerController>();
        if (!player.isDead)
            player.Eliminated();
        gameManager.players.Remove(player);
    }
    
    public override void GameOver()
    {
        foreach (int viewID in gameManager.playersViewID.Values )
        {
            if (!survivedPlayersViewID.Contains(viewID) && PhotonNetwork.IsMasterClient)
                photonView.RPC("EliminatePlayer", RpcTarget.All, viewID);
        }
        gameOverUI.texts[1].text = $"{survivedPlayersViewID.Count}명 생존";
        gameOverUI.gameObject.SetActive(true);
        gameManager.Progress();
    }


}
