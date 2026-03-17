using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void CreateGame()
    {
        SceneManager.LoadSceneAsync(1);
    }


    public void JoinGame()
    {
        SceneManager.LoadSceneAsync(2);
    }


    public void ImportDeck()
    {
        SceneManager.LoadSceneAsync(3);
        Debug.Log("Meep");
    }


    public void Logout()
    {
        SceneManager.LoadSceneAsync(4);
    }
}
