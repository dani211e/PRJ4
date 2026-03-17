using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;


public class SceneLoader : MonoBehaviour
{
    public void LoadDeckViewer()
    {
        SceneManager.LoadScene("DeckViewer");
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
