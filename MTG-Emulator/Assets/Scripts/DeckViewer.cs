using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;

public class DeckViewer : MonoBehaviour
{
    public static DeckViewer Instance;

    [Header("Commander UI")]
    public TMP_Text commanderNameText;
    public Image commanderImage;

    [Header("Card List")]
    public Transform cardListParent;
    public GameObject cardPrefab;

    private DeckDto currentDeck;

    private void Awake() => Instance = this;

    public void ShowDeck(DeckDto deck)
    {
        currentDeck = deck;
        commanderNameText.text = string.IsNullOrEmpty(deck.DeckCommander) ? "Select Commander" : deck.DeckCommander;

        foreach (Transform child in cardListParent)
            Destroy(child.gameObject);

        foreach (CardDto card in deck.Cards)
        {
            GameObject obj = Instantiate(cardPrefab, cardListParent);
            CardUI ui = obj.GetComponent<CardUI>();
            if (ui != null)
                ui.Setup(card, OnCardClicked);
        }
    }

    private void OnCardClicked(CardDto card)
    {
        currentDeck.DeckCommander = card.Name;
        commanderNameText.text = card.Name;

        if (!string.IsNullOrEmpty(card.ImageUri))
            StartCoroutine(LoadImage(card.ImageUri, commanderImage));

        // Persist commander to backend
        StartCoroutine(APIManager.Instance.UpdateDeckCommander(currentDeck.DeckName, card.Name,
            deck => Debug.Log($"Commander updated to {deck.DeckCommander}"),
            error => Debug.LogError("Failed to update commander: " + error)
        ));
    }

    private IEnumerator LoadImage(string url, Image targetImage)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
            Debug.LogError("Image load failed: " + url);
        else
        {
            Texture2D tex = DownloadHandlerTexture.GetContent(request);
            targetImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
    }
}
