using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class KingOfTheHillManager : MiniGameManager
{
    public GameManager gameManager;
    public GameOverUI gameOverUI;
    public Dictionary<int, float> scores = new Dictionary<int, float>();
    public Dictionary<int, PlayerEntry> playerScoreTexts = new Dictionary<int, PlayerEntry>();

    public PlayerEntry playerScoreTextPrefab;
    public RectTransform scoreBoard;
    public Text scoreText;
    public LocalScoreBoard localScoreBoard;
    private int eliminatePlayerCount;
    private int count = 0;
    private void Start()
    {
        eliminatePlayerCount = Mathf.RoundToInt(PhotonNetwork.CurrentRoom.PlayerCount * (2.0f / 3.0f));
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            scores[player.ActorNumber] = 0f;
            PlayerEntry playerScore = Instantiate(playerScoreTextPrefab, scoreBoard, false);
            playerScore.score.text = "0";
            playerScore.name.text = player.NickName;
            playerScoreTexts[player.ActorNumber] = playerScore;
        }
    }

    public override void GameStart()
    {
        foreach (PlayerController player in gameManager.players)
        {
            player.isFreeze = false;
            player.attackForce = 40;
            player.stick.SetActive(true);
            player.stick.layer = default;
        }
    }

    public void UpdateScore(int playerActorNr, float scoreToAdd)
    {
        if (scores.ContainsKey(playerActorNr))
        {
            scores[playerActorNr] += scoreToAdd;
            scoreText.text = scores[playerActorNr].ToString("0");
            photonView.RPC("SyncScore", RpcTarget.All, playerActorNr, scores[playerActorNr]);
        }
    }

    private void Update()
    {
        if (playerScoreTexts[PhotonNetwork.LocalPlayer.ActorNumber].willEliminated.enabled)
        {
            localScoreBoard.scoreUp.color = new Color(1,0.5f,0);
            localScoreBoard.scoreDown.color = Color.red;
        }
        else
        {
            localScoreBoard.scoreUp.color = new Color(0,200f/255f,0);
            localScoreBoard.scoreDown.color = Color.green;
        }
    }

    [PunRPC]
    public void SyncScore(int playerActorNr, float newScore)
    {
        if (scores.ContainsKey(playerActorNr))
        {
            scores[playerActorNr] = newScore;
            UpdateScoreboard();
        }
    }

    private void UpdateScoreboard()
    {
        var sortedScores = scores.OrderByDescending(pair => pair.Value).ToList();

        for (int i = 0; i < sortedScores.Count; i++)
        {
            int playerActorNr = sortedScores[i].Key;
            float playerScore = sortedScores[i].Value;

            playerScoreTexts[playerActorNr].score.text = playerScore.ToString("0");
            playerScoreTexts[playerActorNr].transform.SetSiblingIndex(i);
            playerScoreTexts[playerActorNr].willEliminated.enabled = i >= eliminatePlayerCount;
        }
    }

    [PunRPC]
    void EliminatePlayer(int viewID)
    {
        var player = PhotonView.Find(viewID).GetComponent<PlayerController>();
        player.Eliminated();
        gameManager.players.Remove(player);
    }
    


    public override void GameOver()
    {
        foreach (int actorNumber in playerScoreTexts.Keys)
        {
            if(playerScoreTexts[actorNumber].willEliminated.enabled && PhotonNetwork.IsMasterClient)
                photonView.RPC("EliminatePlayer",RpcTarget.All,gameManager.playersViewID[actorNumber]);
        }
        gameOverUI.texts[1].text = $"{PhotonNetwork.CurrentRoom.PlayerCount - eliminatePlayerCount}명 생존";
        gameOverUI.gameObject.SetActive(true);
        gameManager.Progress();
    }

}
