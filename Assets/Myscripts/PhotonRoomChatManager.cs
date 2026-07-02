using System;
using ExitGames.Client.Photon;
using Photon.Chat;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PhotonRoomChatManager : MonoBehaviourPunCallbacks, IChatClientListener
{
    private const int HistoryLengthToFetch = 20;

    private static PhotonRoomChatManager instance;

    private ChatClient chatClient;
    private string channelName;
    private bool subscribedToChannel;

    public event Action<string, string, string> MessageReceived;
    public event Action<string> StatusChanged;

    public static PhotonRoomChatManager Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }

            instance = FindObjectOfType<PhotonRoomChatManager>();
            if (instance != null)
            {
                return instance;
            }

            GameObject managerObject = new GameObject("Photon Room Chat Manager");
            instance = managerObject.AddComponent<PhotonRoomChatManager>();
            return instance;
        }
    }

    public string ChannelName => channelName;
    public bool IsReady =>
        chatClient != null &&
        subscribedToChannel &&
        !string.IsNullOrWhiteSpace(channelName) &&
        chatClient.CanChatInChannel(channelName);

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        chatClient?.Service();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            chatClient?.Disconnect();
            instance = null;
        }
    }

    public static bool SendToCurrentRoom(string message)
    {
        return Instance.PublishMessage(message);
    }

    public void ConnectToCurrentRoomChannel()
    {
        string targetChannel = ResolveChannelName();
        if (string.IsNullOrWhiteSpace(targetChannel))
        {
            ReportStatus("Photon Chat channel is missing.");
            return;
        }

        if (chatClient != null && channelName == targetChannel)
        {
            if (chatClient.CanChat)
            {
                SubscribeToChannel();
            }

            return;
        }

        chatClient?.Disconnect();
        subscribedToChannel = false;
        channelName = targetChannel;

        ChatAppSettings chatSettings = BuildChatSettings();
        if (string.IsNullOrWhiteSpace(chatSettings.AppIdChat))
        {
            ReportStatus("Photon Chat AppId is missing in PhotonServerSettings.");
            return;
        }

        chatClient = new ChatClient(this, chatSettings.Protocol)
        {
            AuthValues = new Photon.Chat.AuthenticationValues(ResolveUserId()),
            UseBackgroundWorkerForSending = true
        };

        ReportStatus($"Connecting Photon Chat channel '{channelName}'.");
        if (!chatClient.ConnectUsingSettings(chatSettings))
        {
            ReportStatus("Photon Chat connection could not be started.");
        }
    }

    public bool PublishMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return false;
        }

        if (chatClient == null)
        {
            ConnectToCurrentRoomChannel();
            return false;
        }

        if (!IsReady)
        {
            ReportStatus($"Photon Chat is not ready for channel '{channelName}'.");
            return false;
        }

        return chatClient.PublishMessage(channelName, message.Trim());
    }

    public override void OnJoinedRoom()
    {
        ConnectToCurrentRoomChannel();
    }

    public override void OnLeftRoom()
    {
        if (chatClient != null && chatClient.CanChat && subscribedToChannel && !string.IsNullOrEmpty(channelName))
        {
            chatClient.Unsubscribe(new[] { channelName });
        }

        subscribedToChannel = false;
    }

    public new void OnConnected()
    {
        ReportStatus("Photon Chat connected.");
        SubscribeToChannel();
    }

    public void OnDisconnected()
    {
        subscribedToChannel = false;
        ReportStatus("Photon Chat disconnected.");
    }

    public void OnChatStateChange(ChatState state)
    {
        ReportStatus($"Photon Chat state: {state}");
    }

    public void OnGetMessages(string incomingChannelName, string[] senders, object[] messages)
    {
        if (incomingChannelName != channelName)
        {
            return;
        }

        for (int i = 0; i < senders.Length && i < messages.Length; i++)
        {
            string sender = senders[i];
            string message = messages[i]?.ToString() ?? string.Empty;
            Debug.Log($"[PhotonChat:{incomingChannelName}] {sender}: {message}");
            MessageReceived?.Invoke(incomingChannelName, sender, message);
        }
    }

    public void OnPrivateMessage(string sender, object message, string privateChannelName)
    {
        Debug.Log($"[PhotonChat:private:{privateChannelName}] {sender}: {message}");
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        for (int i = 0; i < channels.Length; i++)
        {
            bool result = i < results.Length && results[i];
            if (channels[i] == channelName)
            {
                subscribedToChannel = result;
                ReportStatus(result
                    ? $"Subscribed Photon Chat channel '{channelName}'."
                    : $"Failed to subscribe Photon Chat channel '{channelName}'.");
            }
        }
    }

    public void OnUnsubscribed(string[] channels)
    {
        for (int i = 0; i < channels.Length; i++)
        {
            if (channels[i] == channelName)
            {
                subscribedToChannel = false;
                ReportStatus($"Unsubscribed Photon Chat channel '{channelName}'.");
            }
        }
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
    }

    public void OnUserSubscribed(string subscribedChannelName, string user)
    {
        Debug.Log($"[PhotonChat:{subscribedChannelName}] user subscribed: {user}");
    }

    public void OnUserUnsubscribed(string unsubscribedChannelName, string user)
    {
        Debug.Log($"[PhotonChat:{unsubscribedChannelName}] user unsubscribed: {user}");
    }

    public void DebugReturn(DebugLevel level, string message)
    {
        if (level == DebugLevel.ERROR || level == DebugLevel.WARNING)
        {
            Debug.LogWarning($"Photon Chat {level}: {message}");
        }
        else
        {
            Debug.Log($"Photon Chat {level}: {message}");
        }
    }

    private void SubscribeToChannel()
    {
        if (chatClient == null || !chatClient.CanChat || string.IsNullOrWhiteSpace(channelName))
        {
            return;
        }

        if (subscribedToChannel && chatClient.CanChatInChannel(channelName))
        {
            return;
        }

        chatClient.Subscribe(new[] { channelName }, HistoryLengthToFetch);
    }

    private static ChatAppSettings BuildChatSettings()
    {
        AppSettings punSettings = PhotonNetwork.PhotonServerSettings.AppSettings;
        return new ChatAppSettings
        {
            AppIdChat = punSettings.AppIdChat,
            AppVersion = string.IsNullOrWhiteSpace(PhotonNetwork.GameVersion)
                ? punSettings.AppVersion
                : PhotonNetwork.GameVersion,
            FixedRegion = string.IsNullOrWhiteSpace(punSettings.FixedRegion)
                ? PhotonNetwork.CloudRegion
                : punSettings.FixedRegion,
            NetworkLogging = punSettings.NetworkLogging,
            Protocol = punSettings.Protocol,
            EnableProtocolFallback = punSettings.EnableProtocolFallback,
            Server = punSettings.IsDefaultNameServer ? null : punSettings.Server,
            Port = (ushort)punSettings.Port
        };
    }

    private static string ResolveChannelName()
    {
        if (PhotonNetwork.CurrentRoom != null && !string.IsNullOrWhiteSpace(PhotonNetwork.CurrentRoom.Name))
        {
            return PhotonNetwork.CurrentRoom.Name;
        }

        return LoginSession.HasRoute ? LoginSession.AccountId : string.Empty;
    }

    private static string ResolveUserId()
    {
        if (LoginSession.HasRoute && !string.IsNullOrWhiteSpace(LoginSession.AccountId))
        {
            return LoginSession.AccountId;
        }

        if (PhotonNetwork.LocalPlayer != null && !string.IsNullOrWhiteSpace(PhotonNetwork.LocalPlayer.UserId))
        {
            return PhotonNetwork.LocalPlayer.UserId;
        }

        return PhotonNetwork.NickName;
    }

    private void ReportStatus(string status)
    {
        Debug.Log(status);
        StatusChanged?.Invoke(status);
    }
}
