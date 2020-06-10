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

    [Header("Game")]
    [SerializeField]
    private NetworkGamePlayerLobby _gamePlayerPrefab;
    [SerializeField]
    private GameObject _spawnSystemPrefab;

    public static event Action ClientConnected;
    public static event Action ClientDisconnected;
    public static event Action<NetworkConnection> OnServerReadied;

    public List<NetworkRoomPlayerLobby> RoomPlayers { get; } = new List<NetworkRoomPlayerLobby>();
    public List<NetworkGamePlayerLobby> GamePlayers { get; } = new List<NetworkGamePlayerLobby>();

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

    public void StartGame()
    {
        Scene menuScene = SceneManager.GetSceneByName(_menuSceneName);
        if (menuScene != null && menuScene.isLoaded)
        {
            if (!IsReadyToStart())
            {
                return;
            }

            ServerChangeScene("Scene_Map_01");
        }
    }

    public override void ServerChangeScene(string newSceneName)
    {
        Scene menuScene = SceneManager.GetSceneByName(_menuSceneName);
        if (menuScene == null && menuScene.isLoaded && newSceneName.StartsWith("Scene_Map"))
        {
            for (int i = RoomPlayers.Count - 1; i >= 0; i--)
            {
                var conn = RoomPlayers[i].connectionToClient;
                var gamePlayerInstance = Instantiate(_gamePlayerPrefab);
                gamePlayerInstance.SetDisplayName(RoomPlayers[i].DisplayName);

                NetworkServer.Destroy(conn.identity.gameObject);
                NetworkServer.ReplacePlayerForConnection(conn, gamePlayerInstance.gameObject);
            }
        }

        base.ServerChangeScene(newSceneName);
    }

    public override void OnServerReady(NetworkConnection conn)
    {
        base.OnServerReady(conn);
        OnServerReadied?.Invoke(conn);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (sceneName.StartsWith("Scene_Map"))
        {
            GameObject spawnSystemInstance = Instantiate(_spawnSystemPrefab);
            NetworkServer.Spawn(spawnSystemInstance);
        }
    }
}
