    using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class HideAndSeekManager : MiniGameManager
{
    public GameManager gameManager;
    public GameOverUI gameOverUI;

    private int eliminatePlayerCount = 1;
    public List<int> seeker = new List<int>();
    private void Start()
    {
        eliminatePlayerCount = gameManager.players.Count >= 7 ? 2 : 1;
        if (PhotonNetwork.IsMasterClient)
        {
            while (seeker.Count != eliminatePlayerCount)
            {
                int temp = UnityEngine.Random.Range(0,gameManager.players.Count);
                if (!seeker.Contains(temp))
                {
                    seeker.Add(temp);
                    print("쏘옥");
                }
            }
            photonView.RPC("SyncSeeker",RpcTarget.All,seeker.ToArray());
        }
    }
    [PunRPC]
    void SyncSeeker(int [] seeker)
    {
        this.seeker = seeker.ToList();
    }

    public override void GameStart()
    {
        for (int i = 0;i < gameManager.players.Count; i++)
        {
            bool isSeeker = seeker.Contains(i);
            gameManager.players[i].isFreeze = false;
            if (isSeeker)
                gameManager.players[i].Tagged();
            else
                gameManager.players[i].Untagged();
        }
    }
    


    public override void GameOver()
    {
        foreach(var player in gameManager.players)
        {
            if (player.stick.activeSelf && PhotonNetwork.IsMasterClient)
                player.photonView.RPC("Eliminated",RpcTarget.All);
        }
        
        gameOverUI.texts[1].text = $"{PhotonNetwork.CurrentRoom.PlayerCount - eliminatePlayerCount}명 생존";
        gameOverUI.gameObject.SetActive(true);
        gameManager.Progress();
    }

}
