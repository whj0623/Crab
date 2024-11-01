using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using System.Linq;

public class RoomManager : MonoBehaviourPunCallbacks
{

    public Transform startPositions;
    public ReadyDisplay readyDisplay;
    public string[] gameScenes;
    public RectTransform gameUI;
    public GameObject gameStartUIPrefab;
    public ChatManager chatManager;


    private GameStartUI gameStartUI;

    private Dictionary<int, bool> playersReady = new Dictionary<int, bool>(); // 플레이어들의 준비 상태


    void Start()
    {
        Vector3 spawnPosition = startPositions.GetChild(Random.Range(0, startPositions.childCount)).position;
        GameObject playerObject = PhotonNetwork.Instantiate("Player/Player", spawnPosition, Quaternion.identity);
        gameStartUI = Instantiate(gameStartUIPrefab, gameUI, false).GetComponent<GameStartUI>();
        gameStartUI.UpdatePlayerCountText();
        gameStartUI.UpdateWaitingText(GetReadyPlayerCount());
        chatManager.Connect();
        StartCoroutine(SceneFader.Instance.FadeIn());
        Hashtable customProps = PhotonNetwork.MasterClient.CustomProperties;
        customProps["Ready"] = false;
        customProps["survived"] = true;
        PhotonNetwork.MasterClient.SetCustomProperties(customProps);

    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        readyDisplay.JoinPlayer(newPlayer);
        playersReady[newPlayer.ActorNumber] = false;
        Hashtable customProps = newPlayer.CustomProperties;
        customProps["Ready"] = false;
        customProps["survived"] = true;
        newPlayer.SetCustomProperties(customProps);
        gameStartUI.UpdatePlayerCountText();
        gameStartUI.UpdateWaitingText(GetReadyPlayerCount());

    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        readyDisplay.LeftPlayer(otherPlayer);
        playersReady.Remove(otherPlayer.ActorNumber);
        gameStartUI.UpdatePlayerCountText();
        gameStartUI.UpdateWaitingText(GetReadyPlayerCount());
    }


    public void SetPlayerReady(int actorNumber, bool isReady)
    {
        playersReady[actorNumber] = isReady;
        readyDisplay.SetPlayerReady(actorNumber, isReady);
        gameStartUI.UpdateReadyStateText(playersReady[PhotonNetwork.LocalPlayer.ActorNumber]);
        gameStartUI.UpdateWaitingText(GetReadyPlayerCount());
        OnReadyStateChanged();
    }
    public int GetReadyPlayerCount()
    {
        return playersReady.Values.Count(isReady => isReady);
    }
    private void OnReadyStateChanged()
    {
        if (PhotonNetwork.IsMasterClient && AllPlayersReady())
            StartGame();
    }

    private bool AllPlayersReady()
    {
        return playersReady.Values.All(x => x);
    }

    private void StartGame()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        photonView.RPC("LoadGameScene", RpcTarget.All, Random.Range(0,3));
    }

    [PunRPC]
    private void LoadGameScene(int index)
    {
        StartCoroutine(SceneFader.Instance.FadeOut(()=> PhotonNetwork.LoadLevel(gameScenes[index])));
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (changedProps.ContainsKey("Ready"))
            SetPlayerReady(targetPlayer.ActorNumber, (bool)changedProps["Ready"]);
    }
}


