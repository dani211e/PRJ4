using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ImportPopupUI : MonoBehaviour
{
    [Header("Popup Settings")]
    public GameObject popupRoot;
    public TMP_InputField importField;
    public TMP_InputField deckNameField;

    // Make sure method is public, has no parameters, and is in a MonoBehaviour attached to a GameObject
    public void OpenPopup()
    {
        if (popupRoot != null)
            popupRoot.SetActive(true);
    }

    public void ClosePopup()
    {
        if (popupRoot != null)
            popupRoot.SetActive(false);
    }

    public void SubmitDeck()
    {
        if (importField == null || deckNameField == null)
        {
            Debug.LogError("Input fields are not assigned!");
            return;
        }

        string deckName = deckNameField.text.Trim();
        string importText = importField.text.Trim();

        if (string.IsNullOrEmpty(deckName) || string.IsNullOrEmpty(importText))
        {
            Debug.LogError("Deck name or import is empty!");
            return;
        }

        // Create the deck DTO
        CreateDeckDto dto = new CreateDeckDto
        {
            DeckName = deckName,
            PlayerName = "TestUser", // placeholder
            CardList = importText,
            Commander = ""
        };

        if (APIManager.Instance == null)
        {
            Debug.LogError("APIManager not found in the scene!");
            return;
        }

        // Call APIManager coroutine
        StartCoroutine(APIManager.Instance.CreateDeck(
            dto,
            deck => StartCoroutine(LoadDeckScene(deck)),
            error => Debug.LogError("API ERROR: " + error)
        ));

        ClosePopup();
    }

    private IEnumerator LoadDeckScene(DeckDto deck)
    {
        // Make sure the scene is included in build settings
        AsyncOperation op = SceneManager.LoadSceneAsync("DeckViewer");
        while (!op.isDone)
            yield return null;

        // Wait one frame to ensure DeckViewer instance is initialized
        yield return null;

        if (DeckViewer.Instance != null)
        {
            DeckViewer.Instance.ShowDeck(deck);
        }
        else
        {
            Debug.LogError("DeckViewer.Instance not found after scene load!");
        }
    }
}
