using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/* 
*  This file is part of the Unity networking tutorial by M2H (http://www.M2H.nl)
*  The original author of this code is Mike Hergaarden, even though some small parts 
*  are copied from the Unity tutorials/manuals.
*  Feel free to use this code for your own projects, drop us a line if you made something exciting! 
*/
public class Chat : MonoBehaviour {
    public bool usingChat = false;
    GUISkin skin;
    bool showChat = false;

    private string inputField = "";

    private Vector2 scrollPosition;
    private int width = 500;
    private int height = 180;
    private string playerName;
    private float lastUnfocusTime = 0.0f;
    private Rect window;

    class PlayerNode
    {
        public string playerName;
        public NetworkPlayer networkPlayer;
    }
    private IList<PlayerNode> playerList = new List<PlayerNode>();

    class ChatEntry
    {
        public string name = "";
	    public string text = "";	
    }
    private IList<ChatEntry> chatEntries;

    void Awake() {
        window = new Rect(Screen.width / 2 - width / 2, Screen.height - height + 5, width, height);
        playerName = PlayerPrefs.GetString("playerName", "");
        if (playerName == "")
        {
            playerName = "RandomName" + Random.Range(1, 999);
        }
    }

    //Client function
    void OnConnectedToServer()
    {
        ShowChatWindow();
        GetComponent<NetworkView>().RPC("TellServerOurName", RPCMode.Server, playerName);
    }


    //Server function
    void OnServerInitialized()
    {
        ShowChatWindow();
        //I wish Unity supported sending an RPC on the server to the server itself :(
        // If so; we could use the same line as in "OnConnectedToServer();"
        PlayerNode newEntry = new PlayerNode();
        newEntry.playerName = playerName;
        newEntry.networkPlayer = Network.player;
        playerList.Add(newEntry);
        addGameChatMessage(playerName + " joined the chat");
    }

    PlayerNode GetPlayerNode(NetworkPlayer networkPlayer)
    {
        foreach (PlayerNode entry  in  playerList)
        {
            if (entry.networkPlayer == networkPlayer)
            {
                return entry;
            }
        }
        Debug.LogError("GetPlayerNode: Requested a playernode of non-existing player!");
        return null;
    }

    void OnPlayerDisconnected(NetworkPlayer player)
    {
        addGameChatMessage("Player disconnected from: " + player.ipAddress + ":" + player.port);

        //Remove player from the server list
        playerList.Remove(GetPlayerNode(player));
    }

    void OnDisconnectedFromServer()
    {
        CloseChatWindow();
    }

    //Server function
    void OnPlayerConnected(NetworkPlayer player)
    {
        addGameChatMessage("Player connected from: " + player.ipAddress + ":" + player.port);
    }

    [RPC]
    void TellServerOurName(string name, NetworkMessageInfo info)
    {
        PlayerNode newEntry = new PlayerNode();
        newEntry.playerName = name;
        newEntry.networkPlayer = info.sender;
        playerList.Add(newEntry);

        addGameChatMessage(name + " joined the chat");
    }

    void CloseChatWindow()
    {
        showChat = false;
        inputField = "";
        chatEntries = new List<ChatEntry>();
    }

    void ShowChatWindow()
    {
        showChat = true;
        inputField = "";
        chatEntries = new List<ChatEntry>();
    }

    void OnGUI()
    {
        if (!showChat)
        {
            return;
        }

        GUI.skin = skin;

        if (Event.current.type == EventType.keyDown && Event.current.character == '\n' && inputField.Length <= 0)
        {
            if (lastUnfocusTime + 0.25 < Time.time)
            {
                usingChat = true;
                GUI.FocusWindow(5);
                GUI.FocusControl("Chat input field");
            }
        }

        window = GUI.Window(5, window, GlobalChatWindow, "");
    }

    void GlobalChatWindow(int id)
    {

        GUILayout.BeginVertical();
        GUILayout.Space(10);
        GUILayout.EndVertical();

        // Begin a scroll view. All rects are calculated automatically - 
        // it will use up any available screen space and make sure contents flow correctly.
        // This is kept small with the last two parameters to force scrollbars to appear.
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        foreach (ChatEntry entry  in chatEntries)
        {
            GUILayout.BeginHorizontal();
            if (entry.name == "")
            {//Game message
                GUILayout.Label(entry.text);
            }
            else {
                GUILayout.Label(entry.name + ": " + entry.text);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(3);

        }
        // End the scrollview we began above.
        GUILayout.EndScrollView();

        if (Event.current.type == EventType.keyDown && Event.current.character == '\n' && inputField.Length > 0)
        {
            HitEnter(inputField);
        }
        GUI.SetNextControlName("Chat input field");
        inputField = GUILayout.TextField(inputField);


        if (Input.GetKeyDown("mouse 0"))
        {
            if (usingChat)
            {
                usingChat = false;
                GUI.UnfocusWindow();//Deselect chat
                lastUnfocusTime = Time.time;
            }
        }
    }

    void HitEnter(string msg)
    {
        msg = msg.Replace("\n", "");
        GetComponent<NetworkView>().RPC("GlobalChatText", RPCMode.Server, playerName, msg);
        inputField = ""; //Clear line
        GUI.UnfocusWindow();//Deselect chat
        lastUnfocusTime = Time.time;
        usingChat = false;
    }

    [RPC]
    void ApplyGlobalChatText(string name, string msg)
    {
        var entry = new ChatEntry();
        entry.name = name;
        entry.text = msg;

        chatEntries.Add(entry);

        //Remove old entries
        if (chatEntries.Count > 4)
        {
            chatEntries.RemoveAt(0);
        }

        scrollPosition.y = 1000000;
    }

    void addGameChatMessage(string str)
    {
        ApplyGlobalChatText("", str);
        if (Network.connections.Length > 0)
        {
            GetComponent<NetworkView>().RPC("ApplyGlobalChatText", RPCMode.Others, "", str);
        }
    }
}
