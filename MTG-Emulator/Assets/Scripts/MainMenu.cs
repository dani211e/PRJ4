using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using MTG_Emulator.Unity.Db.DTO.PlayerDTO;

public class MainMenu : MonoBehaviour
{
    [Header("Panels")]
    public GameObject profilePopup;
    public GameObject deleteConfirmationPopup;

    [Header("Profile Info")]
    public TMP_Text usernameText;
    public TMP_Text emailText;

    [Header("Stats")]
    public TMP_Text winsText;
    public TMP_Text lossesText;
    public TMP_Text drawsText;
    public TMP_Text totalGamesText;

    [Header("Password Inputs")]
    public TMP_InputField newPasswordInput;
    public TMP_InputField confirmPasswordInput;

    private void Start()
    {
        if (profilePopup != null) profilePopup.SetActive(false);
        if (deleteConfirmationPopup != null) deleteConfirmationPopup.SetActive(false);
    }

    public void OpenProfilePopup()
    {
        if (profilePopup == null)
        {
            Debug.LogError("profilePopup is not assigned in the Inspector.");
            return;
        }

        profilePopup.SetActive(true);
        StartCoroutine(APIManager.Instance.GetPlayerProfile(OnProfileLoaded, Debug.LogError));
    }

    public void CloseProfilePopup()
    {
        if (profilePopup != null) profilePopup.SetActive(false);
    }

    private void OnProfileLoaded(PlayerDto player)
    {
        if (player == null)
        {
            Debug.LogError("OnProfileLoaded: player is null. Check API response deserialization.");
            return;
        }

        if (usernameText != null)
            usernameText.text = "Username: " + player.Username;
        else
            Debug.LogError("usernameText is not assigned in the Inspector.");

        if (emailText != null)
            emailText.text = "Email: " + PlayerPrefs.GetString("email");
        else
            Debug.LogError("emailText is not assigned in the Inspector.");

        if (winsText != null)
            winsText.text = "Wins: " + player.GamesWon;
        else
            Debug.LogError("winsText is not assigned in the Inspector.");

        if (lossesText != null)
            lossesText.text = "Losses: " + player.GamesLost;
        else
            Debug.LogError("lossesText is not assigned in the Inspector.");

        if (drawsText != null)
            drawsText.text = "Draws: " + player.GamesDrawed;
        else
            Debug.LogError("drawsText is not assigned in the Inspector.");

        int totalGames = player.GamesWon + player.GamesLost + player.GamesDrawed;

        if (totalGamesText != null)
            totalGamesText.text = "Total Games: " + totalGames;
        else
            Debug.LogError("totalGamesText is not assigned in the Inspector.");
    }

    public void ChangePassword()
    {
        if (newPasswordInput == null || confirmPasswordInput == null)
        {
            Debug.LogError("Password input fields are not assigned in the Inspector.");
            return;
        }

        if (newPasswordInput.text != confirmPasswordInput.text)
        {
            UIPopup.Instance.Show("Passwords do not match.");
            return;
        }

        if (string.IsNullOrEmpty(newPasswordInput.text))
        {
            UIPopup.Instance.Show("Password cannot be empty.");
            return;
        }

        StartCoroutine(APIManager.Instance.ResetPassword(
            newPasswordInput.text,
            confirmPasswordInput.text,
            () => UIPopup.Instance.Show("Password changed successfully!"),
            err => UIPopup.Instance.Show("Error: " + err)
        ));
    }

    public void OpenDeleteConfirmation()
    {
        if (deleteConfirmationPopup != null)
            deleteConfirmationPopup.SetActive(true);
        else
            Debug.LogError("deleteConfirmationPopup is not assigned in the Inspector.");
    }

    public void CancelDeleteAccount()
    {
        if (deleteConfirmationPopup != null) deleteConfirmationPopup.SetActive(false);
    }

    public void ConfirmDeleteAccount()
    {
        StartCoroutine(APIManager.Instance.DeleteAccount(
            () =>
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                SceneManager.LoadScene("Login screen");
            },
            Debug.LogError
        ));
    }

    public void CreateGame()
    {
        SceneManager.LoadScene("Create game");
    }

    public void JoinGame()
    {
        SceneManager.LoadScene("Join game");
    }

    public void ImportDeck()
    {
        SceneManager.LoadScene("Deck_Viewer");
    }

    public void Logout()
    {
        PlayerPrefs.DeleteKey("jwtToken");
        PlayerPrefs.DeleteKey("username");
        PlayerPrefs.Save();
        SceneManager.LoadScene("Login screen");
    }
}