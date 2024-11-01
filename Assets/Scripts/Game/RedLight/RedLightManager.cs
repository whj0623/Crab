using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class RedLightManager : MiniGameManager
{
    public GameManager gameManager;
    public GameOverUI gameOverUI;
    public Statue statue; // 술래의 위치를 나타내는 Statue
    public float distanceThreshold = 1f; // 1m
    public bool isGameOver = false;

    private float checkingLeftTime = 5; // 관측 시간
    private Dictionary<PhotonView, Vector3> playerPositions = new Dictionary<PhotonView, Vector3>();
    private HashSet<int> eliminatedPlayers = new HashSet<int>(); // 이미 죽인 플레이어 저장
    private bool isObserving = false;

    public override void GameStart()
    {
        foreach (PlayerController player in gameManager.players)
            player.isFreeze = false;
        foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            Hashtable customprop = new Hashtable();
            customprop["survived"] = false;
            player.SetCustomProperties(customprop);
        }
        StartCoroutine(GameRoutine());
    }

    public override IEnumerator GameRoutine()
    {
        while (!isGameOver)
        {
            yield return new WaitForSeconds(Random.Range(3, 7));
            StartCoroutine(statue.Turn());
            
            yield return new WaitForSeconds(0.5f);
            StartObserving(); // 관측 시작
            
            yield return new WaitForSeconds(checkingLeftTime);
            StopObserving(); // 관측 종료
            
            StartCoroutine(statue.Turn());
        }
    }

    private void Update()
    {
        if (isGameOver) return;
        
        foreach (PlayerController player in gameManager.players)
            if (!player.isDead) return;
        GameOver();

        if (!isObserving) return;

        foreach (var player in playerPositions.Keys)
        {
            Vector3 initialPosition = playerPositions[player];
            float distanceMoved = Vector3.Distance(initialPosition, player.transform.position);

            if (distanceMoved >= 0.1f && !IsPlayerHiddenByOthers(player))
            {
                int viewID = player.ViewID;
                if (!eliminatedPlayers.Contains(viewID) && PhotonView.Find(viewID).gameObject.transform.position.z > -50) // 이미 죽이지 않은 플레이어인지 확인
                {
                    eliminatedPlayers.Add(viewID); 
                    photonView.RPC("EliminatePlayer", RpcTarget.All, viewID);
                }
            }
        }
        

    }

    private void StartObserving()
    {
        isObserving = true;
        playerPositions.Clear();
        eliminatedPlayers.Clear(); // 새 관측 시작 시 제거된 플레이어 목록 초기화

        foreach (var player in gameManager.players)
        {
            playerPositions[player.GetComponent<PhotonView>()] = player.transform.position;
        }
    }

    private void StopObserving()
    {
        isObserving = false;
    }

    private bool IsPlayerHiddenByOthers(PhotonView player)
    {
        Vector3 playerPosition = player.transform.position;
        Vector3 hunterPosition = statue.transform.position;

        foreach (var otherPlayer in playerPositions.Keys)
        {
            if (otherPlayer != player)
            {
                Vector3 otherPlayerPosition = otherPlayer.transform.position;
                Vector3 directionToPlayer = (playerPosition - hunterPosition).normalized;
                Vector3 directionToOther = (otherPlayerPosition - hunterPosition).normalized;
                float angle = Vector3.Angle(directionToPlayer, directionToOther);

                if (angle < 15f && Vector3.Distance(playerPosition, otherPlayerPosition) <= distanceThreshold)
                {
                    return true;
                }
            }
        }

        return false;
    }

    [PunRPC]
    public void EliminatePlayer(int viewID)
    {
        PhotonView.Find(viewID).GetComponent<PlayerController>().Eliminated();
    }

    public override void GameOver()
    {
        isGameOver = true;
        foreach (Photon.Realtime.Player player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            if (!(bool)player.CustomProperties["survived"])
                photonView.RPC("EliminatePlayer", RpcTarget.All, player.ActorNumber);
        }

        gameOverUI.texts[1].text = $"{gameManager.leftPlayerCount}명 생존";
        gameOverUI.gameObject.SetActive(true);

        gameManager.Progress();
    }
}
