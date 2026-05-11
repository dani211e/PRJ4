using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class DeckViewer : MonoBehaviour
{
    public static DeckViewer Instance;


    [Header("Deck names to load")] [SerializeField]
    private List<int> decksPreGet = new();

    [Header("Deck UI")]
    [SerializeField] private Transform deckListParent;
    [SerializeField] private GameObject deckButtonPrefab;

    [Header("Commander UI")]
    [SerializeField] private TMP_Text commanderNameText;
    [SerializeField] private Image commanderImage;

    [Header("Card List")]
    [SerializeField] private Transform cardListParent;
    [SerializeField] private GameObject cardRowPrefab;

    [SerializeField] private GameObject deckListPanel;
    [SerializeField] private GameObject deckDetailsPanel;

    private DeckDto currentDeck;

    private void Awake() => Instance = this;


    public void Start()
    {
        LoadDeckList();
    }

    public void LoadDeckList()
    {
        StartCoroutine((APIManager.GetDeckByPlayerId(
            playerId,
            decks =>
            {
                Debug.Log("Loaded decks for player: " + decks.Count),
                    
            })))
    }

    public void AddDeckButton(DeckDto deck)
    {
        GameObject obj = Instantiate(deckButtonPrefab, deckListParent);

        TMP_Text text = obj.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.text = deck.deckName;
            Debug.Log("Set button text to: " + deck.deckName);
        }

        Button button = obj.GetComponentInChildren<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => ShowDeck(deck));
        }
    }

    public void ShowDeckList()
    {
        if (deckDetailsPanel != null)
            deckDetailsPanel.SetActive(false);

        if (deckListPanel != null)
            deckListPanel.SetActive(true);
    }

    public void ShowDeck(DeckDto deck)
    {
        currentDeck = deck;

        if (deckListPanel != null)
        {
            deckListPanel.SetActive(false);
        }

        if (deckDetailsPanel != null)
        {
            deckDetailsPanel.SetActive(true);
        }


        commanderNameText.text = string.IsNullOrEmpty(deck.deckCommander)
            ? "Select Commander"
            : deck.deckCommander;

        if (commanderImage != null)
            commanderImage.sprite = null;

        foreach (Transform child in cardListParent)
            Destroy(child.gameObject);

        var groupedCards = deck.cards
            .GroupBy(c => c.name)
            .OrderBy(g => g.Key);

        foreach (var group in groupedCards)
        {
            GameObject rowObj = Instantiate(cardRowPrefab, cardListParent);

            Transform quantityTransform = rowObj.transform.Find("CardQuantity");
            Transform nameTransform = rowObj.transform.Find("CardName");

            if (quantityTransform != null)
                quantityTransform.GetComponent<TMP_Text>().text = group.Count().ToString();

            if (nameTransform != null)
                nameTransform.GetComponent<TMP_Text>().text = group.Key;
        }

    }

    private void OnCardClicked(CardDto card)
    {
        if (currentDeck == null)
        {
            return;
        }

        currentDeck.deckCommander = card.name;
        commanderNameText.text = card.name;

        if (!string.IsNullOrEmpty(card.imageUri))
            StartCoroutine(LoadImage(card.imageUri, commanderImage));

        // Persist commander to backend
        StartCoroutine(APIManager.Instance.UpdateDeckCommander(currentDeck.deckName, card.name,
            deck => Debug.Log($"Commander updated to {deck.deckCommander}"),
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
    
    public void OnClickBack()
    {
        SceneManager.LoadScene("0");
    }
}
