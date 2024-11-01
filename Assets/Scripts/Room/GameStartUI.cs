using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStartUI : MonoBehaviourPunCallbacks
{
    public Text playerCount;
    public Text waiting;
    public Text readyState;


    public void UpdatePlayerCountText()
    {
        playerCount.text = $"플레이어 {PhotonNetwork.CurrentRoom.PlayerCount}/10";
    }
    
    public void UpdateWaitingText(int readyPlayerCount)
    {
        if (readyPlayerCount == PhotonNetwork.CurrentRoom.PlayerCount)
            waiting.text = "잠시 후 게임을 시작합니다.";
        else
            waiting.text = $"준비를 기다리는중...\n{readyPlayerCount}/{PhotonNetwork.CurrentRoom.PlayerCount}";
    }

    public void UpdateReadyStateText(bool isReady)
    {
        readyState.text = isReady ? "준비 완료" : "대기중";
        readyState.color = isReady ? Color.green : Color.red;
    }

    

}
