using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform deckVisualParent;
    [SerializeField] private Transform handParent;
    [SerializeField] private int deckSize = 100;

    private List<Sprite> cards = new();
    private GameObject topVisual;

    void Awake()
    {
        if (deckVisualParent == null) deckVisualParent = transform;
    }

    void Start()
    {
        var fronts = Resources.LoadAll<Sprite>("FrontImage");
        cards.Clear();

        if (fronts.Length == 0)
        {
            return;
        }

        for (int i = 0; i < deckSize; i++)
        {
            cards.Add(fronts[i % fronts.Length]);
        }

        ShowTopFaceDown();
    }
    

    public void DrawToHand()
    {
        if (cards.Count == 0) return;

        var front = cards[^1];
        cards.RemoveAt(cards.Count - 1);

        var go = Instantiate(cardPrefab, handParent, false);
        go.GetComponent<Card>().Init(front, startFaceDown: false);
        
        
        ShowTopFaceDown();
    }
    
    void ShowTopFaceDown()
    {
        if (topVisual == null)
        {
            topVisual = Instantiate(cardPrefab, deckVisualParent, false);
        }
        
        var drag = topVisual.GetComponent<Drag>();
        if (drag != null)
        {
            Destroy(drag);
        }

        var c = topVisual.GetComponent<Card>();
        c.Init(front: cards.Count > 0 ? cards[^1] : null, startFaceDown: true);
    }
}