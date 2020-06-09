using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManagerLobby : NetworkManager
{
    [SerializeField]
    private int _minPlayers;

    [Scene]
    [SerializeField]
    private string _menuScenePath;
    private string _menuSceneName;

    [Header("Room")]
    [SerializeField]
    private NetworkRoomPlayerLobby _roomPlayerPrefab;

    public static event Action ClientConnected;
    public static event Action ClientDisconnected;

    public List<NetworkRoomPlayerLobby> RoomPlayers { get; } = new List<NetworkRoomPlayerLobby>();

    public override void Start()
    {
        base.Start();
        _menuSceneName = StripSceneName(_menuScenePath);
    }

    public override void OnClientConnect(NetworkConnection conn)
    {

        base.OnClientConnect(conn);
        ClientConnected?.Invoke();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        if (conn.identity != null)
        {
            NetworkRoomPlayerLobby player = conn.identity.GetComponent<NetworkRoomPlayerLobby>();
            RoomPlayers.Remove(player);


        }

        base.OnClientDisconnect(conn);
        ClientDisconnected?.Invoke();
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        if (numPlayers >= maxConnections)
        {
            conn.Disconnect();
            return;
        }

        Scene menuScene = SceneManager.GetSceneByName(_menuSceneName);
        //Prevents player from joining to the game that has already started. This is what the tutorial does, but we don't want it in final version of the HUB game. TODO: delete after the tutorial
        if (menuScene == null || !menuScene.isLoaded)
        {
            conn.Disconnect();
            return;
        }

        base.OnServerConnect(conn);
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        Scene menuScene = SceneManager.GetSceneByName(_menuSceneName);
        if (menuScene != null && menuScene.isLoaded)
        {
            bool isLeader = RoomPlayers.Count == 0;

            GameObject roomPlayerObject = Instantiate(_roomPlayerPrefab.gameObject);
            NetworkRoomPlayerLobby roomPlayer = roomPlayerObject.GetComponent<NetworkRoomPlayerLobby>();
            roomPlayer.IsLeader = isLeader;

            NetworkServer.AddPlayerForConnection(conn, roomPlayerObject);
        }
    }

    private string StripSceneName(string scenePath)
    {
        string[] pathParts = scenePath.Split('/');
        string fileName = pathParts[pathParts.Length - 1]; //get last part of the path
        fileName = fileName.Split('.')[0]; // remove extension
        return fileName;
    }

    public override void OnStopServer()
    {
        RoomPlayers.Clear();
    }

    private bool IsReadyToStart()
    {
        if (RoomPlayers.Count < _minPlayers)
        {
            return false;
        }

        foreach (NetworkRoomPlayerLobby player in RoomPlayers)
        {
            if (!player.IsReady)
            {
                return false;
            }
        }

        return true;
    }

    public void NotifyPlayersOfReadyState()
    {
        foreach (NetworkRoomPlayerLobby player in RoomPlayers)
        {
            player.HandleReadyToStart(IsReadyToStart());
        }
    }
}
