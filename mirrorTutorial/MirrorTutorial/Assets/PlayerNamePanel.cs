using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerNamePanel : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField _nameInput;
    [SerializeField]
    private Button _continueButton;
    [SerializeField]
    private Button _saveNameButton;
    [SerializeField]
    private GameObject _landingPagePanel;

    public static string DisplayName { get; private set; }

    private const string PLAYER_NAME_KEY = "PlayerName";

    private void Start()
    {
        SetContinueButtonState(false);
        SetupInputField();

        _saveNameButton.onClick.AddListener(OnSaveNameButtonClicked);
    }

    private void SetupInputField()
    {
        if (PlayerPrefs.HasKey(PLAYER_NAME_KEY))
        {
            _nameInput.text = PlayerPrefs.GetString(PLAYER_NAME_KEY, "Player");
        }

        SavePlayerName(_nameInput.text);
    }

    private void SavePlayerName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return;
        }

        PlayerPrefs.SetString(PLAYER_NAME_KEY, name);
        SetContinueButtonState(true);
    }

    private void SetContinueButtonState(bool enabled)
    {
        _continueButton.interactable = enabled;
    }

    private void OnSaveNameButtonClicked()
    {
        SavePlayerName(_nameInput.text);
        _landingPagePanel.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }

}
