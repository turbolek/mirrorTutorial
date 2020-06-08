using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinLobbyMenu : MonoBehaviour
{
    [SerializeField]
    private NetworkManagerLobby _networkManager;
    [SerializeField]
    private GameObject _landingPagePanel;
    [SerializeField]
    private TMP_InputField _ipAddressInput;
    [SerializeField]
    private Button _joinButton;

    private void Start()
    {
        _joinButton.onClick.AddListener(JoinLobby);
    }

    private void OnEnable()
    {
        NetworkManagerLobby.ClientConnected += HandleClientConnected;
        NetworkManagerLobby.ClientDisconnected += HandleClientDisconnected;
    }
    private void OnDisable()
    {
        NetworkManagerLobby.ClientConnected -= HandleClientConnected;
        NetworkManagerLobby.ClientDisconnected -= HandleClientDisconnected;
    }

    private void HandleClientConnected()
    {
        _joinButton.interactable = true;
        gameObject.SetActive(false);
        _landingPagePanel.SetActive(false);
    }

    private void HandleClientDisconnected()
    {
        _joinButton.interactable = true;
    }

    private void JoinLobby()
    {
        string ipAddress = _ipAddressInput.text;

        _networkManager.networkAddress = ipAddress;
        _networkManager.StartClient();

        _joinButton.interactable = false;
    }

}
