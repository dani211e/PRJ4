using System.Collections.Generic;
using System.Linq;
using MTG_Emulator;
using MTG_Emulator.Cards;
using MTG_Emulator.Cards.Extensions;
using MTG_Emulator.Unity.Db.DTO.DeckDTO;
using TMPro;
using UnityEngine;
using Random = System.Random;

public class Deck : MonoBehaviour
{
    [SerializeField]
    private TMP_Text countText;

    [SerializeField]
    private Transform handParent;

    [SerializeField]
    private GameObject cardPrefab;


    private List<CardInfo> drawPile = new();

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
                    drawPile.AddRange(result.Cards.Select(c => c.ToCardInfo()));
                }
                
                shuffle();

                UpdateCountText();

                Debug.Log("Loaded gameplay deck: " + result.DeckName);
                Debug.Log("Cards loaded: " + drawPile.Count);

                ResetGameplayStateForNewDeck();
            }, error => { Debug.LogError("Failed to load cards from deck " + deck.DeckId + " " + error); }));
    }

    private void ResetGameplayStateForNewDeck()
    {
    }

    public void DrawCard()
    {
        CardInfo card = DrawTopCard();

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

        CardManager.AddObject(GameSession.PlayerId, card, cardObj);
    }

    public CardInfo DrawTopCard()
    {
        if (drawPile.Count == 0)
        {
            Debug.LogWarning("No cards left in draw pile.");
            return null;
        }

        CardInfo card = drawPile[0];
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

    private void shuffle()
    {
        var rng = new Random();
        var n = drawPile.Count;
        while (n > 1)
        {
            n--;
            var k = rng.Next(n + 1);
            (drawPile[k], drawPile[n]) = (drawPile[n], drawPile[k]);
        }
        Debug.Log("Deck shuffled.");
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
            shuffle();
    }
}