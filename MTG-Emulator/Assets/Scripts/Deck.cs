using System.Collections.Generic;
using MTG_Emulator.Unity.Db.DTO.CardDTO;
using MTG_Emulator.Unity.Db.DTO.DeckDTO;
using TMPro;
using UnityEngine;

public class Deck : MonoBehaviour
{
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Transform handParent;
    [SerializeField] private GameObject cardPrefab;
    

    private DeckDto currentDeck;
    private List<CardDto> drawPile = new();

    public void LoadDeck(DeckDto deck)
    {
        if (deck == null)
        {
            Debug.LogError("Tried to load a null deck.");
            return;
        }

        currentDeck = deck;
        drawPile.Clear();

        if (deck.Cards != null)
            drawPile = new List<CardDto>(deck.Cards);

        UpdateCountText();

        Debug.Log("Loaded gameplay deck: " + currentDeck.DeckName);
        Debug.Log("Cards loaded: " + drawPile.Count);

        ResetGameplayStateForNewDeck();
    }

    private void ResetGameplayStateForNewDeck()
    {
    }

    public void DrawCard()
    {
        CardDto card = DrawTopCard();

        if (card == null)
            return;

        if (handParent == null)
        {
            Debug.LogError("Hand parent is not assigned.");
            return;
        }

        if (cardPrefab == null)
        {
            Debug.LogError("Card prefab is not assigned.");
            return;
        }

        GameObject cardObj = Instantiate(cardPrefab, handParent);

        Card cardScript = cardObj.GetComponent<Card>();
        if (cardScript != null)
            cardScript.Setup(card);
        else
            Debug.LogError("Card prefab does not have a Card script.");
    }

    public CardDto DrawTopCard()
    {
        if (drawPile.Count == 0)
        {
            Debug.LogWarning("No cards left in draw pile.");
            return null;
        }

        CardDto card = drawPile[0];
        drawPile.RemoveAt(0);
        UpdateCountText();
        return card;
    }

    public int GetRemainingCardCount()
    {
        return drawPile.Count;
    }

    private void UpdateCountText()
    {
        if (countText != null)
            countText.text = drawPile.Count.ToString();
    }
}