using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private int eliminatePlayerCount;

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
        PhotonView.Find(viewID).GetComponent<PlayerController>().Eliminated();
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
                print("À¸¾Ç");
        }
    }

    public override void GameOver()
    {
        SetSurvivalStatus();

        foreach (Photon.Realtime.Player player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if (!(bool)player.CustomProperties["survived"])
            {
                EliminatePlayer(player.ActorNumber);
            }
        }

        gameOverUI.texts[1].text = $"{gameManager.leftPlayerCount}¸í »ýÁ¸";
        gameOverUI.gameObject.SetActive(true);

        gameManager.Progress();
    }
}
