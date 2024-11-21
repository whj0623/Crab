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
    public Statue statue; // ������ ��ġ�� ��Ÿ���� Statue
    public float distanceThreshold = 1f; // 1m
    public bool isGameOver = false;

    private float checkingLeftTime = 5; // ���� �ð�
    private Dictionary<PhotonView, Vector3> playerPositions = new Dictionary<PhotonView, Vector3>();
    private HashSet<int> eliminatedPlayers = new HashSet<int>(); // �̹� ���� �÷��̾� ����
    private bool isObserving = false;

    public AudioSource redLightAudioSource;
    public AudioSource greenLightAudioSource;

    public override void GameStart()
    {
        survivedPlayersViewID = new List<int>();
        foreach (PlayerController player in gameManager.players)
            player.isFreeze = false;
        StartCoroutine(GameRoutine());
    }

    public override IEnumerator GameRoutine()
    {
        while (!isGameOver)
        {
            yield return new WaitForSeconds(Random.Range(3, 7));
            StartCoroutine(statue.Turn());
            redLightAudioSource.Play();
            yield return new WaitForSeconds(1f);  
            StartObserving();
            
            yield return new WaitForSeconds(checkingLeftTime);
            StopObserving(); 
            
            StartCoroutine(statue.Turn());
            greenLightAudioSource.Play();

            
        }
    }

    private void Update()
    {
        if (isGameOver) return;
        
        foreach (var player in playerPositions.Keys)
        {
            Vector3 initialPosition = playerPositions[player];
            float distanceMoved = Vector3.Distance(initialPosition, player.transform.position);

            if (distanceMoved >= 0.1f && !IsPlayerHiddenByOthers(player) && isObserving)
            {
                int viewID = player.ViewID;
                if (!eliminatedPlayers.Contains(viewID) && !survivedPlayersViewID.Contains(viewID)) 
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
        eliminatedPlayers.Clear();

        foreach (var player in gameManager.players)
            playerPositions[player.GetComponent<PhotonView>()] = player.transform.position;
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
                    return true;
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
