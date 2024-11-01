using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPanel : MonoBehaviour
{
	public RectTransform roomListRect;
	private List<RoomInfo> currentRoomList = new List<RoomInfo>();
	public RoomInformation roomPrefab;
	public Button closeButton;

	[Header("방 비밀번호 입력")]
	public GameObject roomPWInputUI;
	public InputField roomPWInput;
	public Button confirmPWButton;
	public Button cancelButton;

	private void Awake()
	{
		closeButton.onClick.AddListener(closeButtonClick);
		cancelButton.onClick.AddListener(() => { roomPWInputUI.SetActive(false); });
	}

	public void UpdateRoomList(List<RoomInfo> roomList)
	{
		List<RoomInfo> destroyCandidate = currentRoomList.FindAll((x) => false == roomList.Contains(x));
		foreach (RoomInfo roomInfo in roomList)
		{
			if (currentRoomList.Contains(roomInfo) || !roomInfo.IsOpen) 
				continue;
			AddRoom(roomInfo);
		}

		foreach (Transform child in roomListRect)
		{
			if (destroyCandidate.Exists((x) => x.Name == child.name))
				Destroy(child.gameObject);
		}

		currentRoomList = roomList;

	}

	public void AddRoom(RoomInfo roomInfo)
	{
		RoomInformation newRoom = Instantiate(roomPrefab, roomListRect, false);
		newRoom.gameObject.name = roomInfo.Name;
		newRoom.roomName.text = roomInfo.Name.ToString();
		newRoom.playerCount.text = $"{roomInfo.PlayerCount}/{roomInfo.MaxPlayers}";
		foreach (DictionaryEntry entry in roomInfo.CustomProperties)
		{
			Debug.Log($"Custom Property: {entry.Key} = {entry.Value}");
		}

		newRoom.isPrivateImage.gameObject.SetActive(roomInfo.CustomProperties.ContainsKey("password"));

		newRoom.gameObject.GetComponentInChildren<Button>().onClick.AddListener(() =>
		{
			RoomInfo selectedRoomInfo = roomInfo;
            if (selectedRoomInfo.CustomProperties.ContainsKey("password"))
                ShowPasswordInputUI(selectedRoomInfo);
            else
                PhotonNetwork.JoinRoom(selectedRoomInfo.Name);
		});
		void ShowPasswordInputUI(RoomInfo selectedRoomInfo)
		{
			roomPWInputUI.SetActive(true); 
			confirmPWButton.onClick.RemoveAllListeners(); 
			confirmPWButton.onClick.AddListener(() =>
			{
				confirmPWButton.interactable = false;
				string enteredPassword = roomPWInput.text;
				string roomPassword = selectedRoomInfo.CustomProperties["password"].ToString();

				if (enteredPassword == roomPassword)
				{
					roomPWInputUI.SetActive(false);
					PhotonNetwork.JoinRoom(selectedRoomInfo.Name);
				}
				else
				{
					PanelManager.Instance.Dialog("비밀번호가 틀렸습니다.");
					roomPWInput.text = string.Empty;
				}
				confirmPWButton.interactable = true;
			});
		}
		newRoom.GetComponentInChildren<Text>().text = roomInfo.Name;
	}

	private void closeButtonClick()
	{
		PhotonNetwork.LeaveLobby();
	}

}
