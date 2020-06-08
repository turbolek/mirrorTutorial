using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    private NetworkManager _networkManager;

    [Header("UI")]
    [SerializeField] private GameObject _landingPagePanel;
    [SerializeField] private GameObject _joinLobbyPanel;

    [SerializeField]
    private Button _hostButton;
    [SerializeField]
    private Button _joinButton;

    private void Start()
    {
        _hostButton.onClick.AddListener(HostLobby);
        _joinButton.onClick.AddListener(JoinLobby);
    }

    public void HostLobby()
    {
        _networkManager.StartHost();
        _landingPagePanel.SetActive(false);
    }

    public void JoinLobby()
    {
        _joinLobbyPanel.SetActive(true);
        _landingPagePanel.SetActive(false);
    }
}
