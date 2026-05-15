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
    

    private List<CardDto> drawPile = new();

    public void LoadDeck(DeckDto deck)
    {
        
        if (deck == null)
        {
            Debug.LogError("Tried to load a null deck.");
            return;
        }
        
        StartCoroutine(APIManager.Instance.GetDeckById(
            deck.DeckId,
            result =>
            {
                Debug.Log(result.Cards);
                
                drawPile.Clear();

                if (result.Cards != null)
                {
                    drawPile.AddRange(result.Cards);
                }
                UpdateCountText();

                Debug.Log("Loaded gameplay deck: " + result.DeckName);
                Debug.Log("Cards loaded: " + drawPile.Count);

                ResetGameplayStateForNewDeck();
                
            }, error =>
            {
                Debug.LogError("Failed to load cards from deck " + deck.DeckId + " " + error);

            }));
        
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