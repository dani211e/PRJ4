using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MTG_Emulator.Backend.DB.Models;
using MTG_Emulator.Unity.Db.DTO.CardDTO;
using MTG_Emulator.Unity.Db.DTO.DeckDTO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class DeckViewer : MonoBehaviour
{
    public static DeckViewer Instance;

    [Header("Deck UI")]
    [SerializeField]
    private Transform deckListParent;

    [SerializeField]
    private GameObject deckButtonPrefab;

    [Header("Commander UI")]
    [SerializeField]
    private TMP_Text commanderNameText;

    [SerializeField]
    private Image commanderImage;

    [SerializeField]
    private Transform commanderPanel;

    [SerializeField]
    private GameObject commanderEntryPrefab;

    [Header("Card List")]
    [SerializeField]
    private Transform cardListParent;

    [SerializeField]
    private GameObject cardRowPrefab;

    [Header("Panels")]
    [SerializeField]
    private GameObject menuPanel;

    [SerializeField]
    private GameObject deckListPanel;

    [SerializeField]
    private GameObject deckDetailsPanel;

    private DeckDto currentDeck;
    private List<CardDto> selectedCommanders = new();

    private void Awake() => Instance = this;


    public void Start()
    {
        Debug.Log("APIManager instance: " + APIManager.Instance);
        Debug.Log("Saved username: " + PlayerPrefs.GetString("username"));

        if (menuPanel != null)
            menuPanel.SetActive(true);
        if (deckListPanel != null)
            deckListPanel.SetActive(false);
        if (deckDetailsPanel != null)
            deckDetailsPanel.SetActive(false);

        LoadDeckList();
    }

    public void LoadDeckList()
    {
        foreach (Transform child in deckListParent)
            Destroy(child.gameObject);

        var username = PlayerPrefs.GetString("username");

        StartCoroutine(APIManager.Instance.GetDecksByUsername(
            username,
            result =>
            {
                foreach (DeckDto item in result)
                    addDeckButton(item);
            },
            error => Debug.LogError("Failed to load decks: " + error)
        ));
    }

    private void addDeckButton(DeckDto deck)
    {
        GameObject obj = Instantiate(deckButtonPrefab, deckListParent);

        TMP_Text text = obj.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.text = deck.DeckName;
            Debug.Log("Set button text to: " + deck.DeckName);
        }

        Button button = obj.GetComponentInChildren<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => ShowDeck(deck));
        }
    }

    // Called by DeckListBackButton goes back to MenuPanel
    public void ShowMenuPanel()
    {
        if (deckListPanel != null)
            deckListPanel.SetActive(false);
        if (deckDetailsPanel != null)
            deckDetailsPanel.SetActive(false);
        if (menuPanel != null)
            menuPanel.SetActive(true);
    }

    // Called by DeckDetailsBackButton  goes back to deck list
    public void ShowDeckList()
    {
        if (deckDetailsPanel != null)
            deckDetailsPanel.SetActive(false);
        if (deckListPanel != null)
            deckListPanel.SetActive(true);
    }

    public void ShowDeck(DeckDto deck)
    {
        Debug.Log("ShowDeck called for: " + deck.DeckName);
        currentDeck = deck;
        selectedCommanders = deck.CommandZone.ToList();

        if (deckListPanel != null)
            deckListPanel.SetActive(false);
        if (deckDetailsPanel != null)
            deckDetailsPanel.SetActive(true);

        commanderNameText.text = selectedCommanders.Count > 0
            ? string.Join(", ", selectedCommanders.Select(c => c.Name))
            : "Select Commander";

        if (commanderImage != null)
            commanderImage.sprite = null;

        StartCoroutine(APIManager.Instance.GetDeckById(
            deck.DeckId,
            result =>
            {
                selectedCommanders = result.CommandZone?.ToList() ?? new List<CardDto>();

                foreach (Transform child in cardListParent)
                    Destroy(child.gameObject);

                var groupCards = result.Cards.GroupBy(card => card.Name).OrderBy(g => g.Key);

                foreach (var group in groupCards)
                {
                    GameObject rowObj = Instantiate(cardRowPrefab, cardListParent);

                    Transform quantityTransform = rowObj.transform.Find("CardQuantity");
                    Transform nameTransform = rowObj.transform.Find("CardName");

                    if (quantityTransform != null)
                        quantityTransform.GetComponent<TMP_Text>().text = group.Count().ToString();

                    if (nameTransform != null)
                        nameTransform.GetComponent<TMP_Text>().text = group.Key;
                }

                foreach (Transform child in commanderPanel)
                    Destroy(child.gameObject);

                float yOffset = 0f;
                foreach (var commander in selectedCommanders)
                {
                    GameObject entry = Instantiate(commanderEntryPrefab, commanderPanel);
                    RectTransform rt = entry.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0, 1);
                    rt.anchorMax = new Vector2(1, 1);
                    rt.pivot = new Vector2(0.5f, 1);
                    rt.offsetMin = new Vector2(10, -yOffset - 400);
                    rt.offsetMax = new Vector2(-10, -yOffset);
                    yOffset += 420f;

                    TMP_Text nameText = entry.GetComponentInChildren<TMP_Text>();
                    if (nameText != null)
                        nameText.text = commander.Name;

                    Image img = entry.transform.Find("CardImage")?.GetComponent<Image>();
                    if (img != null && !string.IsNullOrEmpty(commander.ImageUri))
                        StartCoroutine(LoadImage(commander.ImageUri, img));
                }
            },
            error => Debug.LogError("Failed to load cards from deck " + deck.DeckId + " " + error)
        ));
    }

    private IEnumerator LoadImage(string url, Image targetImage)
    {
        if (!url.StartsWith("http"))
            url = "http://localhost:5042" + url;

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

    public void SelectCommander(CardDto card)
    {
        if (selectedCommanders.Any(c => c.Name == card.Name))
            selectedCommanders.RemoveAll(c => c.Name == card.Name);
        else
            selectedCommanders.Add(card);

        commanderNameText.text = selectedCommanders.Count > 0
            ? string.Join(", ", selectedCommanders.Select(c => c.Name))
            : "No Commander";

        if (selectedCommanders.Count > 0 && !string.IsNullOrEmpty(selectedCommanders[0].ImageUri))
            StartCoroutine(LoadImage(selectedCommanders[0].ImageUri, commanderImage));
        else if (commanderImage != null)
        {
            commanderImage.sprite = null;
            commanderImage.color = Color.white;
        }

        StartCoroutine(APIManager.Instance.UpdateDeckCommander(
            currentDeck.DeckId,
            selectedCommanders.Select(c => c.Name).ToList(),
            currentDeck.DeckName,
            string.Join("\n", currentDeck.Cards.Select(c => $"1 {c.Name}")),
            result =>
            {
                if (result != null)
                    Debug.Log("Commander updated: " + string.Join(", ", result.CommandZone.Select(c => c.Name)));
                else
                    Debug.Log("Commander updated successfully.");
            },
            error => Debug.LogError("Failed to update commander: " + error)
        ));
    }

    public void OnClickBack()
    {
        SceneManager.LoadScene("0");
    }
}