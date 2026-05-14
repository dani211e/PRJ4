using System;
using System.Collections;
using MTG_Emulator.Unity.Db.DTO.DeckDTO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ImportPopupUI : MonoBehaviour
{
    [Header("Popup Settings")]
    [SerializeField] private GameObject importPopup;
    [SerializeField] private GameObject deckListPopup;
    [SerializeField] private GameObject deckCardsPopup;
    public TMP_InputField importField;
    public TMP_InputField deckNameField;

    // Make sure method is public, has no parameters, and is in a MonoBehaviour attached to a GameObject
    public void OpenImportPopup()
    {
        if ( importPopup != null)
            importPopup.SetActive(true);
    }

    public void OpenDeckListPopup()
    {
        if ( deckListPopup != null)
            deckListPopup.SetActive(true);
    }

    public void OpenDeckCardsPopup()
    {
        if ( deckCardsPopup != null)
            deckCardsPopup.SetActive(true);
    }

    public void ClosePopup()
    {
        if (importPopup != null)
            importPopup.SetActive(false);

        if (deckListPopup != null)
            deckListPopup.SetActive(false);

        if (deckCardsPopup != null)
            deckCardsPopup.SetActive(false);
    }


    public void SubmitDeck()
    {
        if (importField == null || deckNameField == null)
        {
            Debug.LogError("Input fields are not assigned!");
            return;
        }

        string deckName = deckNameField.text.Trim();
        string importText = importField.text
            .Replace("\r\n", "\n")
            .Replace("\r", "\n")
            .Trim();

        string[] lines = importText.Split("\n", StringSplitOptions.RemoveEmptyEntries);

        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = lines[i].Trim();
        }

        importText = string.Join("\n", lines);

        Debug.Log("Deck name: "  + deckName);
        Debug.Log("Card list:\n " + importText);

        if (string.IsNullOrEmpty(deckName) || string.IsNullOrEmpty(importText))
        {
            Debug.LogError("Deck name or import is empty");
            return;
        }

        // Create the deck DTO
        CreateDeckDto dto = new CreateDeckDto
        {
            DeckName = deckName,
            CardList = importText,
            Commander = "none"
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
        AsyncOperation op = SceneManager.LoadSceneAsync("Deck_Viewer");
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
