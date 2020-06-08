using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManagerLobby : NetworkManager
{
    [Scene]
    [SerializeField]
    private string _menuScenePath;
    private string _menuSceneName;

    [Header("Room")]
    [SerializeField]
    private NetworkRoomPlayerLobby _roomPlayerPrefab;

    public static event Action ClientConnected;
    public static event Action ClientDisconnected;

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
            GameObject roomPlayer = Instantiate(_roomPlayerPrefab.gameObject);
            NetworkServer.AddPlayerForConnection(conn, roomPlayer);
        }
    }

    private string StripSceneName(string scenePath)
    {
        string[] pathParts = scenePath.Split('/');
        string fileName = pathParts[pathParts.Length - 1]; //get last part of the path
        fileName = fileName.Split('.')[0]; // remove extension
        return fileName;
    }

}
