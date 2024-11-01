using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelManager : MonoBehaviourPunCallbacks
{
	public static PanelManager Instance { get; private set; }

	public LoginPanel login;
	public MenuPanel menu;
	public LobbyPanel lobby;

	public Dialog dialog;

	private Dictionary<string, GameObject> panels;

	#region Unity ¸Þ½ÃÁö
	private void Awake()
	{
		Instance = this;
		panels = new Dictionary<string, GameObject> 
		{
			{ "Login", login.gameObject },
			{ "Menu", menu.gameObject },
			{ "Lobby", lobby.gameObject},
		};
		PanelOpen("Login");
		PhotonNetwork.AddCallbackTarget(this);
	}

	#endregion

	public void PanelOpen(string panelName)
	{
		foreach (var row in panels)
			row.Value.SetActive(row.Key.Equals(panelName));
	}

	public override void OnConnected()
	{
		PanelOpen("Menu");
		PanelManager.Instance.dialog.gameObject.SetActive(false);
	}

	public override void OnDisconnected(DisconnectCause cause)
	{
		PanelOpen("Login");
	}

	public override void OnJoinedLobby()
	{
		PanelOpen("Lobby");
	}

	public override void OnLeftLobby()
	{
		PanelOpen("Menu");
	}

	public override void OnJoinedRoom()
	{

		StartCoroutine(SceneFader.Instance.FadeOut(() => SceneManager.LoadScene("RoomScene")));
		
		PhotonNetwork.AutomaticallySyncScene = true;
	}

	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		lobby.UpdateRoomList(roomList);
	}

	public void Dialog(string msg)
    {
		dialog.text.text = msg;
		dialog.gameObject.SetActive(true);
    }

}
