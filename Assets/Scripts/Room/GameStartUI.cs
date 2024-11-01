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
        playerCount.text = $"�÷��̾� {PhotonNetwork.CurrentRoom.PlayerCount}/10";
    }
    
    public void UpdateWaitingText(int readyPlayerCount)
    {
        if (readyPlayerCount == PhotonNetwork.CurrentRoom.PlayerCount)
            waiting.text = "��� �� ������ �����մϴ�.";
        else
            waiting.text = $"�غ� ��ٸ�����...\n{readyPlayerCount}/{PhotonNetwork.CurrentRoom.PlayerCount}";
    }

    public void UpdateReadyStateText(bool isReady)
    {
        readyState.text = isReady ? "�غ� �Ϸ�" : "�����";
        readyState.color = isReady ? Color.green : Color.red;
    }

    

}
