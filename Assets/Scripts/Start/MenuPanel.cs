using Newtonsoft.Json;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Hashtable = ExitGames.Client.Photon.Hashtable;
public class MenuPanel : MonoBehaviour
{
	[Header("Main Menu")]
	public RectTransform mainMenuUI;
	public Button createRoomButton;
	public Button findRoomButton;
	public Button logoutButton;

	[Header("Create Room Menu")]
	public RectTransform createRoomMenuUI;
	public InputField roomNameInput;
	public InputField roomPWInput;
	public Toggle privateRoomToggle;
	public Button createButton;
	public Button cancelButton;



	private void Awake()
	{
		createRoomButton.onClick.AddListener(CreateRoomButtonClick);
		findRoomButton.onClick.AddListener(FindRoomButtonClick);
		logoutButton.onClick.AddListener(LogoutButtonClick);
		createButton.onClick.AddListener(CreateButtonClick);
		cancelButton.onClick.AddListener(CancelButtonClick);
		privateRoomToggle.onValueChanged.AddListener((isOn) => roomPWInput.interactable = (privateRoomToggle.isOn));
	}

	private void OnEnable()
	{
		mainMenuUI.gameObject.SetActive(true);
		createRoomMenuUI.gameObject.SetActive(false);
	}

	private void CreateRoomButtonClick()
	{
		mainMenuUI.gameObject.SetActive(false);
		createRoomMenuUI.gameObject.SetActive(true);
	}

	private void FindRoomButtonClick()
	{
		PhotonNetwork.JoinLobby();
	}

	private void LogoutButtonClick()
	{
		mainMenuUI.gameObject.SetActive(false);
		PhotonNetwork.Disconnect();
	}

	private void CreateButtonClick()
	{
		string roomName = roomNameInput.text;
		Hashtable customProperties = new();
		if (string.IsNullOrEmpty(roomName))
		{
			PanelManager.Instance.Dialog("방 이름을 입력하세요.");
			roomNameInput.text = string.Empty;
			return;
		}
		if (privateRoomToggle.isOn)
        {
			customProperties.Add("password", roomPWInput.text);
			
        }
		PanelManager.Instance.Dialog("방을 만드는 중입니다..");
		PanelManager.Instance.dialog.closeButton.interactable = false;

		PhotonNetwork.CreateRoom(roomName,new RoomOptions(){ MaxPlayers = 10,CustomRoomProperties = customProperties});
	}

	private void CancelButtonClick()
	{
		mainMenuUI.gameObject.SetActive(true);
		createRoomMenuUI.gameObject.SetActive(false);
	}
}
