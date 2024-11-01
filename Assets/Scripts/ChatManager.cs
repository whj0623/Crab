using ExitGames.Client.Photon;
using Photon.Chat;
using Photon.Pun;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChatManager : MonoBehaviourPunCallbacks, IChatClientListener
{
    public ChatClient chatClient;
    public ScrollRect scrollRect;
    public GameObject messagePrefab; 
    public Transform contentTransform;
    public InputField chatInputField;
    private string currentChannel;
    private bool isInputFieldActive = false;
    private bool isActive = false;

    public void Connect()
    {
        currentChannel = PhotonNetwork.CurrentRoom.Name;
        chatClient = new ChatClient(this);
        chatClient.Connect("d11bd8d9-1214-4fed-9c4f-be7455b3b085", chatClient.AppVersion, null);
        chatClient.Subscribe(new string[] { currentChannel });
        chatInputField.interactable = false;
        DontDestroyOnLoad(gameObject);
        isActive = true;
    }


    private void Update()
    {
        if (!isActive) return;
        chatClient.Service();
        
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!isInputFieldActive)
            {
                chatInputField.ActivateInputField();
                isInputFieldActive = true;
            }
            else if (string.IsNullOrEmpty(chatInputField.text))
            { 
                chatInputField.DeactivateInputField();
                isInputFieldActive = false;
                EventSystem.current.SetSelectedGameObject(null);
            }
            else
            {
                SendMessageToChannel(chatInputField.text);
                chatInputField.text = string.Empty;
                chatInputField.ActivateInputField();
            }
        }
    }
    private void SendMessageToChannel(string message)
    {
        if (chatClient != null && chatClient.CanChat)
            chatClient.PublishMessage(currentChannel,$"{PhotonNetwork.LocalPlayer.NickName} : {message}");
    }


    public void DebugReturn(DebugLevel level, string message)
    {

    }

    public void OnChatStateChange(ChatState state)
    {

    }

    public void OnDisconnected()
    {

    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for (int i = 0; i < senders.Length; i++)
            AddMessageToChat(senders[i], messages[i].ToString());
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {

    }
    public override void OnConnected()
    {
        chatClient.Subscribe(new string[] { currentChannel });
    }
    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {

    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        chatInputField.interactable = true;
        chatClient.PublishMessage(currentChannel, $"{PhotonNetwork.LocalPlayer.NickName} ´ÔÀÌ ÀÔÀåÇÏ¼Ì½À´Ï´Ù.");
    }

    public void OnUnsubscribed(string[] channels)
    {

    }

    public void OnUserSubscribed(string channel, string user)
    {

    }

    public void OnUserUnsubscribed(string channel, string user)
    {

    }

    private void AddMessageToChat(string sender, string message)
    {
        GameObject messageGO = Instantiate(messagePrefab, contentTransform);
        Text messageText = messageGO.GetComponent<Text>();
        messageText.text = message;
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0;
    }
}
