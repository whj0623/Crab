using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;


public class GameManager : MonoBehaviourPunCallbacks
{
    public Transform startPositions;
    public Timer[] timers;
    public Button startButton;
    public GameObject mapCamera;
    public GameObject introUI;
    public GameObject statusUI;
    public List<PlayerController> players;
    public MiniGameManager minigameManager;
    private string[] gameScenes ={"Small Glass Jump","Small Color Climb", "Desert","Karlson"};
    public Dictionary<int, int> playersViewID;
    private int playersReady = 0;
    public Minigames CurrentGame ;


    private void Awake()
    {
        startButton.onClick.AddListener(() => StartCoroutine(Ready()));
        players = new List<PlayerController>();
        playersViewID = new Dictionary<int, int>();
    }

    IEnumerator Ready()
    {
        startButton.enabled = false;
        StartCoroutine(SceneFader.Instance.FadeOut(0,null));
        yield return new WaitForSeconds(2f);
        photonView.RPC("PlayerReady", RpcTarget.MasterClient);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        mapCamera.SetActive(false);
        introUI.SetActive(false);
        if (statusUI != null)
            statusUI.SetActive(true);
        if ((bool)PhotonNetwork.LocalPlayer.CustomProperties["survived"] == true)
        {
            int playerNumber = PhotonNetwork.LocalPlayer.ActorNumber - 1;
            Transform playerPos = startPositions.GetChild(playerNumber);
            GameObject playerObj = PhotonNetwork.Instantiate("Player/Player", playerPos.position, playerPos.rotation); 
            playerObj.name = $"Player {PhotonNetwork.LocalPlayer.NickName}";
            PlayerController playerController = playerObj.GetComponent<PlayerController>();
            playerController.currentGame = CurrentGame;
            int viewID = playerController.GetComponent<PhotonView>().ViewID;
            playerController.isFreeze = true;
            photonView.RPC("AddPlayer", RpcTarget.All,playerNumber+1,viewID);
        }
        else
            SetSpectatorMode();

        StartCoroutine(SceneFader.Instance.FadeIn());
    }

    [PunRPC]
    void AddPlayer(int actorNumber,int viewID) 
    {
        StartCoroutine(WaitForPlayerInstantiate(viewID));
        playersViewID.Add(actorNumber,viewID);
    }
    IEnumerator WaitForPlayerInstantiate(int viewID)
    {
        while (true)
        {
            PhotonView pv = PhotonView.Find(viewID);
            if (pv != null)
            {
                PlayerController pc = pv.GetComponent<PlayerController>();
                if (pc != null)
                {
                    players.Add(pc);
                    yield break;
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    
    
    
    private void SetSpectatorMode()
    {

    }

    private void Start()
    {
        if (PhotonNetwork.InRoom)
            StartCoroutine(GameIntro());
    }

    private IEnumerator GameIntro()
    {
        yield return new WaitUntil(() => PhotonNetwork.LocalPlayer.GetPlayerNumber() != -1);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        StartCoroutine(SceneFader.Instance.FadeIn());
    }


    
    public void Progress()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (players.Count > 1)
                photonView.RPC("LoadNextGameScene", RpcTarget.All, ChooseNextGame());
            //else
            //{
            //     
            //}
        }
    }

    [PunRPC]
    private void LoadNextGameScene(int index)
    {
        StartCoroutine(SceneFader.Instance.FadeOut(3,() => PhotonNetwork.LoadLevel(gameScenes[index])));
    }

    private int ChooseNextGame()
    {
        Minigames nextGame = CurrentGame;
        while (CurrentGame == nextGame)
            nextGame = (Minigames)UnityEngine.Random.Range(0, 4);
        return (int)nextGame;
    }

   [PunRPC]
    void PlayerReady()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            playersReady++;
            if (playersReady == PhotonNetwork.PlayerList.Length)
                photonView.RPC("SyncTimer", RpcTarget.All);
        }
    }

    [PunRPC]
    void SyncTimer()
    {
        if (timers != null)
        {
            foreach (Timer timer in timers)
                timer.timerIsRunning = true;
        }
    }
}
