using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NetworkRoomPlayerLobby : NetworkBehaviour
{
    [Header("UI")]
    [SerializeField]
    private GameObject _lobbyUI = null;
    [SerializeField]
    private TMP_Text[] _playerNameTexts = new TMP_Text[4];
    [SerializeField]
    private TMP_Text[] _playerReadyTexts = new TMP_Text[4];
    [SerializeField]
    private Button _startGameButton = null;
    [SerializeField]
    private Button _readyButton = null;

    [SyncVar(hook = nameof(HandleDisplayNameChanged))]
    public string DisplayName = "Loading...";
    [SyncVar(hook = nameof(HandleReadyStatusChanged))]
    public bool IsReady = false;

    private NetworkManagerLobby _room;

    private NetworkManagerLobby Room
    {
        get
        {
            if (_room != null)
            {
                return _room;
            }

            return _room = NetworkManager.singleton as NetworkManagerLobby;
        }
    }


    private bool _isLeader;
    public bool IsLeader
    {
        set
        {
            _isLeader = value;
            _startGameButton.gameObject.SetActive(value);
        }
        get
        {
            return _isLeader;
        }
    }

    private void Start()
    {
        _startGameButton.onClick.AddListener(CmdStartGame);
        _readyButton.onClick.AddListener(CmdReadyUp);
    }

    public override void OnStartAuthority()
    {
        CmdSetDisplayName(PlayerNamePanel.DisplayName);
        _lobbyUI.SetActive(true);
    }

    public override void OnStartClient()
    {
        Room.RoomPlayers.Add(this);
        UpdateDisplay();
    }

    public override void OnStopClient()
    {
        Room.RoomPlayers.Remove(this);
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (!hasAuthority)
        {
            foreach (NetworkRoomPlayerLobby player in Room.RoomPlayers)
            {
                if (player.hasAuthority)
                {
                    player.UpdateDisplay();
                    break;
                }
            }
            return;
        }

        for (int i = 0; i < _playerNameTexts.Length; i++)
        {
            _playerNameTexts[i].text = "Waiting for player...";
            _playerReadyTexts[i].text = string.Empty;
        }

        for (int i = 0; i < Room.RoomPlayers.Count; i++)
        {
            _playerNameTexts[i].text = Room.RoomPlayers[i].DisplayName;
            _playerReadyTexts[i].text = Room.RoomPlayers[i].IsReady ?
                "<color=green>Ready</color>" : "<color=red>Not Ready</color>";
        }
    }

    public void HandleReadyToStart(bool readyToStart)
    {
        if (!IsLeader)
        {
            return;
        }

        _startGameButton.interactable = readyToStart;
    }

    public void HandleReadyStatusChanged(bool oldValue, bool newValue)
    {
        UpdateDisplay();
    }
    public void HandleDisplayNameChanged(string oldValue, string newValue)
    {
        UpdateDisplay();
    }

    [Command]
    private void CmdSetDisplayName(string displayName)
    {
        DisplayName = displayName;
    }

    [Command]
    public void CmdReadyUp()
    {
        IsReady = !IsReady;

        Room.NotifyPlayersOfReadyState();
    }

    [Command]
    public void CmdStartGame()
    {
        if (Room.RoomPlayers[0].connectionToClient != connectionToClient)
        {
            return;
        }

        Room.StartGame();
    }

}
