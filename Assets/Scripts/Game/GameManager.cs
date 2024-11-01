using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class GameManager : MonoBehaviourPunCallbacks
{
    public Transform startPositions;
    public Timer[] timers;
    public Button startButton;
    public GameObject mapCamera;
    public GameObject introUI;
    public GameObject statusUI;
    public int leftPlayerCount;
    public Text leftPlayerCountText;
    public Text deadPlayerCountText;
    public List<PlayerController> players;
    public MiniGameManager minigameManager;
    public string[] gameScenes ={"Small Glass Jump","Small Color Climb", "Desert","Splat","Karlson"};
    
    private int playersReady = 0;

    public int CurrentGame = 0;


    private void Awake()
    {
        startButton.onClick.AddListener(() => StartCoroutine(Ready()));
        players = new List<PlayerController>();
        leftPlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        leftPlayerCountText.text = leftPlayerCount.ToString();
        deadPlayerCountText.text = "0";
    }

    IEnumerator Ready()
    {
        startButton.enabled = false;
        StartCoroutine(SceneFader.Instance.FadeOut());
        yield return new WaitForSeconds(2f);
        photonView.RPC("PlayerReady", RpcTarget.MasterClient);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        mapCamera.SetActive(false);
        introUI.SetActive(false);
        statusUI.SetActive(true);
        if ((bool)PhotonNetwork.LocalPlayer.CustomProperties["survived"] == true)
        {
            int playerNumber = PhotonNetwork.LocalPlayer.ActorNumber - 1;
            Transform playerPos = startPositions.GetChild(playerNumber);
            GameObject playerObj = PhotonNetwork.Instantiate("Player/Player", playerPos.position, playerPos.rotation); 
            playerObj.name = $"Player {PhotonNetwork.LocalPlayer.NickName}";
            PlayerController playerController = playerObj.GetComponent<PlayerController>();
            playerController.OnPlayerDead += OnPlayerDead;
            playerController.isFreeze = true;
            players.Add(playerController);
        }
        else
            SetSpectatorMode();

        StartCoroutine(SceneFader.Instance.FadeIn());
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

    private void OnPlayerDead()
    {
        leftPlayerCount--;
        leftPlayerCountText.text = leftPlayerCount.ToString();
        deadPlayerCountText.text = (PhotonNetwork.CurrentRoom.PlayerCount - leftPlayerCount).ToString();
    }

    
    public void Progress()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (leftPlayerCount > 1)
                photonView.RPC("LoadNextGameScene", RpcTarget.All, ChooseNextGame());
            //else
            //{
            //   우승자가 있을 경우 => 우승 씬
            //   없는 경우 => Room으로 돌아감
            //}
        }
    }

    [PunRPC]
    private void LoadNextGameScene(int index)
    {
        StartCoroutine(SceneFader.Instance.FadeOut(() => PhotonNetwork.LoadLevel(gameScenes[index])));
    }

    private int ChooseNextGame()
    {
        int i = CurrentGame;
        while (CurrentGame == i)
            i = UnityEngine.Random.Range(0, 3);
        return i;
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
