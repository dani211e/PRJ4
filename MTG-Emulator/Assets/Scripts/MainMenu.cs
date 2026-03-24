    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine.SceneManagement;

    public class MainMenu : MonoBehaviour
    {
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
            SceneManager.LoadScene("Login screen");
        }
    }
