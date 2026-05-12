using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckSelectPopup : MonoBehaviour
{
    [SerializeField] private GameObject settingsPopup;
    [SerializeField] private GameObject deckSelectionPopup;
    [SerializeField] private Transform deckListParent;
    [SerializeField] private GameObject deckButtonPrefab;
    [SerializeField] private Deck gameplayDeck;
    private int CurrecntPlayerId = 1;


    public void OpenSettingsPopup()
    {
        if (settingsPopup != null)
            settingsPopup.SetActive(true);
    }

    public void OpenDeckSelectionPopup()
    {
        if (deckSelectionPopup != null)
            deckSelectionPopup.SetActive(true);

        LoadDeckList();
    }

    public void ClosePopup()
    {
        if (settingsPopup != null)
            settingsPopup.SetActive(false);

        if (deckSelectionPopup != null)
            deckSelectionPopup.SetActive(false);

    }

    public void LoadDeckList()
    {
        foreach (Transform child in deckListParent)
        {
            Destroy(child.gameObject);
        }

        StartCoroutine(APIManager.Instance.GetDecksByPlayerId(
            CurrecntPlayerId,
            result =>
            {
                foreach (DeckDto item in result)
                {
                    AddDeckButton(item);
                }
            },
            error =>
            {
                Debug.LogError("Failed to load decks " + error);
            }
        ));
    }

    private void AddDeckButton(DeckDto deck)
    {
        GameObject obj = Instantiate(deckButtonPrefab, deckListParent);

        TMP_Text text = obj.GetComponentInChildren<TMP_Text>(true);
        if (text != null)
            text.text = deck.deckName;

        Button button = obj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                gameplayDeck.LoadDeck(deck);
                ClosePopup();
            });
        }
    }
}
