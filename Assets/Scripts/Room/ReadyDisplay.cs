using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReadyDisplay : MonoBehaviourPunCallbacks
{
    public RectTransform playerList;
    public GameObject playerTextPrefab;
    public Dictionary<int, PlayerEntry> playerEntries = new();

    private void Start()
    {
        foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values)
            JoinPlayer(player);
    }

    public void JoinPlayer(Player player)
    {
        var playerEntry = Instantiate(playerTextPrefab, playerList, false).GetComponent<PlayerEntry>();
        playerEntry.name.text = player.NickName;
        playerEntry.gameObject.name = player.NickName;
        playerEntries[player.ActorNumber] = playerEntry;
        SortPlayers();
    }

    public void LeftPlayer(Player player)
    {
        GameObject leaveTarget = playerEntries[player.ActorNumber].gameObject;
        playerEntries.Remove(player.ActorNumber);
        Destroy(leaveTarget);
        SortPlayers();
    }

    public void SortPlayers()
    {
        foreach (int actorNumber in playerEntries.Keys)
            playerEntries[actorNumber].transform.SetSiblingIndex(actorNumber - 1);
    }

    public void SetPlayerReady(int actorNumber, bool isReady)
    {
        playerEntries[actorNumber].GetComponent<Image>().color = isReady ? Color.green : Color.red;
    }

    public void ReadyButtonInteract(bool isOn)
    {
        Player localPlayer = PhotonNetwork.LocalPlayer;
        Hashtable customProps = localPlayer.CustomProperties;
        customProps["Ready"] = isOn;
        localPlayer.SetCustomProperties(customProps);
    }
}
