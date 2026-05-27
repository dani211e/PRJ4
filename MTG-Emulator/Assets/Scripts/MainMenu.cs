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
            profilePopup.SetActive(false);
            deleteConfirmationPopup.SetActive(false);
        }

        public void OpenProfilePopup()
        {
            profilePopup.SetActive(true);

            StartCoroutine(APIManager.Instance.GetPlayerProfile(OnProfileLoaded, Debug.LogError));
        }

        public void CloseProfilePopup()
        {
            profilePopup.SetActive(false);
        }

        private void OnProfileLoaded(PlayerDto player)
        {

                usernameText.text = "Username: " + player.Username;
                emailText.text = "Email: " + PlayerPrefs.GetString("email");
                winsText.text = "Wins: " + player.GamesWon;
                lossesText.text = "Losses: " + player.GamesLost;
                drawsText.text = "Draws: " + player.GamesDrawed;

                int totalGames = player.GamesWon + player.GamesLost + player.GamesDrawed;
                totalGamesText.text = "Total Games: " + totalGames;
                
        }
        public void ChangePassword()
        {
            if (newPasswordInput.text != confirmPasswordInput.text)
            {
                UIPopup.Instance.Show("Passwords do not match.");
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
            deleteConfirmationPopup.SetActive(true);
        }

        public void CancelDeleteAccount()
        {
            deleteConfirmationPopup.SetActive(false);
        }

        public void ConfirmDeleteAccount()
        {
            StartCoroutine(APIManager.Instance.DeleteAccount(
                () =>
                {
                    PlayerPrefs.DeleteAll();
                    PlayerPrefs.DeleteKey("jwtToken");
                    PlayerPrefs.DeleteKey("username");
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
