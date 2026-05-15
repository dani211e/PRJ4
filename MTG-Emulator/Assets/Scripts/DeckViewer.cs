using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        
        Debug.Log("APIManager instance: " + APIManager.Instance);
        Debug.Log("Saved username: " + PlayerPrefs.GetString("username"));
        
        LoadDeckList();
    }

    public void LoadDeckList()
    {
        foreach (Transform child in deckListParent)
        {
            Destroy(child.gameObject);
        }
        
        string username = PlayerPrefs.GetString("username");
        

        StartCoroutine(APIManager.Instance.GetDecksByUsername(
            username,
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

    public void AddDeckButton(DeckDto deck)
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

    public void ShowDeckList()
    {
        if (deckDetailsPanel != null)
            deckDetailsPanel.SetActive(false);

        if (deckListPanel != null)
            deckListPanel.SetActive(true);
    }

    public void ShowDeck(DeckDto deck)
    {
        
        if (deckListPanel != null)
        {
            deckListPanel.SetActive(false);
        }

        if (deckDetailsPanel != null)
        {
            deckDetailsPanel.SetActive(true);
        }
        
        commanderNameText.text = string.IsNullOrEmpty(deck.DeckCommander) ? "Select Commander" : deck.DeckCommander;

        if (commanderImage != null)
        {
            commanderImage.sprite = null;
        }


        StartCoroutine(APIManager.Instance.GetDeckById(
            deck.DeckId,
            result =>
            {
                foreach (Transform child in cardListParent)
                {
                    Destroy(child.gameObject);
                }

                var groupCards = deck.Cards.GroupBy(card => card.Name).OrderBy(g => g.Key);

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
            }, error =>
            {
                Debug.LogError("Failed to load cards from deck " + deck.DeckId + " " + error);

            }));
            
        
        
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
