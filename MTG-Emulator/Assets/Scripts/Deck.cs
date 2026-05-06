using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
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

        if (deck.cards != null)
            drawPile.AddRange(deck.cards);

        Debug.Log("Loaded gameplay deck: " + currentDeck.deckName);
        Debug.Log("Cards loaded: " + drawPile.Count);

        ResetGameplayStateForNewDeck();
    }

    private void ResetGameplayStateForNewDeck()
    {

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
        return card;
    }

    public DeckDto GetCurrentDeck()
    {
        return currentDeck;
    }

    public int GetRemainingCardCount()
    {
        return drawPile.Count;
    }
}
