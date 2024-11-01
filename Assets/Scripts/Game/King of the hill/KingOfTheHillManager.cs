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
    public void EliminatePlayer(int viewID)
    {
        var photonView = PhotonView.Find(viewID);
        if (photonView == null)
        {
            Debug.LogError($"No PhotonView found with ID {viewID}");
            return;
        }

        var playerController = photonView.GetComponent<PlayerController>();
        if (playerController == null)
        {
            Debug.LogError("PlayerController component is missing on the player object.");
            return;
        }

        playerController.Eliminated();
    }

    public void SetSurvivalStatus()
    {
        var sortedScores = scores.OrderBy(pair => pair.Value).ToList();
        for (int i = 0; i < PhotonNetwork.CurrentRoom.PlayerCount; i++)
        {
            int playerActorNr = sortedScores[i].Key;
            Photon.Realtime.Player player = PhotonNetwork.CurrentRoom.GetPlayer(playerActorNr);

            if (player != null)
            {
                Hashtable props = new Hashtable { { "survived", i >= eliminatePlayerCount } };
                player.SetCustomProperties(props);
            }
            else
                print("����");
        }
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        count++;
    }

    [PunRPC]
    public void ActivateGameOverUI()
    {
        gameOverUI.texts[1].text = $"{gameManager.leftPlayerCount}명 생존";
        gameOverUI.gameObject.SetActive(true);
    }

    
    public override void GameOver()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        SetSurvivalStatus();
        StartCoroutine(EliminatePlayers());
        
    }

    private IEnumerator EliminatePlayers()
    {
        yield return new WaitUntil(() => count == PhotonNetwork.CurrentRoom.PlayerCount);
        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if (!(bool)player.CustomProperties["survived"])
                 photonView.RPC("EliminatePlayer", RpcTarget.All, gameManager.playersViewID[player.ActorNumber]);
        }
        photonView.RPC("ActivateGameOverUI", RpcTarget.All);
        gameManager.Progress();
    }
}
