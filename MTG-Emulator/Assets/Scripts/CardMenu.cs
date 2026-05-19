using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using MTG_Emulator.Backend.DB.Models;
using MTG_Emulator.Unity.Db.DTO.CardDTO;
using MTG_Emulator.Unity.Db.DTO.RelatedCardDTO;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class CardMenu : MonoBehaviour
{
    public static CardMenu Instance;
    private Card currentCard;
    [SerializeField] private GameObject toggleCardMenu;
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private GameObject createToken;
    [SerializeField] private Transform battlefieldParent;
    [SerializeField] private Transform cardMenuParent;
    
    

    private void Awake() => Instance = this;

    private void Start() => toggleCardMenu.SetActive(false);
        

    public void Open(Card card)
    {
        currentCard = card;
        float buttonHeight = 30f;
        float spacing = 10f;
        
        for(int i = 0; i < currentCard.cardData.RelatedCards.Count; i++)
        {
            RelatedCardDto relatedCard = currentCard.cardData.RelatedCards[i];
            var button = Instantiate(createToken, cardMenuParent);
            RectTransform buttonRect = button.GetComponent<RectTransform>();
            buttonRect.anchoredPosition = new Vector2(0, -i * (buttonHeight + spacing));
            var buttonTransform = button.GetComponent<Button>();
            
            buttonTransform.GetComponentInChildren<TextMeshProUGUI>().text = relatedCard.Name;

            int index = i;
            buttonTransform.onClick.AddListener(() => OnSpawnTokenClicked(index));
        }
        toggleCardMenu.SetActive(true);
    }

    private void Close()
    {
        foreach (Transform child in cardMenuParent)
        {
            Destroy(child.gameObject);
        }
        toggleCardMenu.SetActive(false);
    } 

    public void OnSpawnTokenClicked(int index)
    {
        if (currentCard.cardData.RelatedCards.Count == 0)
        {
            Debug.Log("This card has no tokens to spawn.");
            return;
        }
        RelatedCardDto token = currentCard.cardData.RelatedCards[index];
        GameObject tokenObj = Instantiate(cardPrefab, battlefieldParent);
        Token tokenScript = tokenObj.GetComponent<Token>();
        Debug.Log(token.ImageUri);
        tokenScript.Setup(token);
        Close();
    }
    
}