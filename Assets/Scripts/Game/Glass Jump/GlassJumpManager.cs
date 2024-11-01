using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlassJumpManager : MiniGameManager
{
    public SafeZone safeZone;
    public GameManager gameManager;
    public GameOverUI gameOverUI;
    public void GameStart()
    {
        foreach (PlayerController player in gameManager.players)
            player.isFreeze = false;
    }

    public override void GameOver()
    {
        foreach (Photon.Realtime.Player player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if (!(bool)player.CustomProperties["survived"])
                photonView.RPC("EliminatePlayer", RpcTarget.All, player.ActorNumber);
        }

        gameOverUI.texts[1].text = $"{gameManager.leftPlayerCount}¸í »ýÁ¸";
        gameOverUI.gameObject.SetActive(true);

        gameManager.Progress();
    }


}
