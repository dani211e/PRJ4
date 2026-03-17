using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void CreateGame()
    {
        SceneManager.LoadSceneAsync("Create game");
    }


    public void JoinGame()
    {
        SceneManager.LoadSceneAsync("Join game");
    }


    public void ImportDeck()
    {
        SceneManager.LoadSceneAsync("Deck_Viewer");
    }


    public void Logout()
    {
        SceneManager.LoadSceneAsync("Login screen");
    }
}
