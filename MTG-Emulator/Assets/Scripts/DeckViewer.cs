using System.Collections.Generic;
using System.Linq;
using MTG_Emulator.Unity.Db.DTO.CardDTO;
using MTG_Emulator.Unity.Db.DTO.DeckDTO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [Header("Delete Deck")]
    [SerializeField] private GameObject deleteDeckConfirmationPopup;
    
    private DeckDto currentDeck;
    private List<CardDto> selectedCommanders = new();

    private void Awake() => Instance = this;


    public void Start()
    {
        Debug.Log("APIManager instance: " + APIManager.Instance);
        Debug.Log("Saved username: " + PlayerPrefs.GetString("username"));

        if (!ensureValidUI())
            Destroy(this);

        menuPanel.SetActive(true);
        deckListPanel.SetActive(false);
        deckDetailsPanel.SetActive(false);

        LoadDeckList();
    }

    private bool ensureValidUI()
    {
        bool valid = true;

        Validate(deckDetailsPanel);
        Validate(deckListPanel);
        Validate(menuPanel);
        Validate(commanderImage.gameObject);
        
        return valid;

        void Validate(GameObject obj)
        {
            if (obj)
                return;

            Debug.LogError($"{obj.name} was not set, ensure it is not null.");
            valid = false;
        }
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
        deckListPanel.SetActive(false);
        deckDetailsPanel.SetActive(false);
        menuPanel.SetActive(true);
    }

    // Called by DeckDetailsBackButton  goes back to deck list
    public void ShowDeckList()
    {
        deckDetailsPanel.SetActive(false);
        deckListPanel.SetActive(true);
    }

    public void ShowDeck(DeckDto deck)
    {
        Debug.Log("ShowDeck called for: " + deck.DeckName);
        currentDeck = deck;
        selectedCommanders = deck.CommandZone.ToList();

        deckListPanel?.SetActive(false);
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
                        StartCoroutine(APIManager.Instance.LoadImage(commander.ImageUri, img));
                }
            },
            error => Debug.LogError("Failed to load cards from deck " + deck.DeckId + " " + error)
        ));
    }

    public void OnClickDeleteDec()
    {
        deleteDeckConfirmationPopup.SetActive(true);
    }
    public void CancelDeleteDeck()
    {
        deleteDeckConfirmationPopup.SetActive(false);
    }

    public void ConfirmDeleteDeck()
    {
        StartCoroutine(APIManager.Instance.DeleteDeck(
            currentDeck.DeckId,
            () =>
            {
                deleteDeckConfirmationPopup.SetActive(false);
                currentDeck = null;
                LoadDeckList();
                ShowDeckList();
            },
            error => Debug.LogError("Failed to delete deck: " + error)
        ));
    }

    public void OnClickBack()
    {
        SceneManager.LoadScene("0");
    }
}