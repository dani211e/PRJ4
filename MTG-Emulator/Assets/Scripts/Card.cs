using System;
using System.Collections;
using MTG_Emulator.Cards;
using TMPro;
using UnityEngine;
using MTG_Emulator.Backend.DB.Models;
using MTG_Emulator.Unity.Db.DTO.CardDTO;
using UnityEngine.EventSystems;
using MTG_Emulator.Backend.DB.Models;
using MTG_Emulator.Unity.Db.DTO.CardDTO;
using MTG_Emulator.Unity.Db.DTO.RelatedCardDTO;
using MTG_Emulator.Unity.Synchronization.Enums;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Card : MonoBehaviour, IPointerClickHandler
{
    [Header("Visible UI")]
    [SerializeField]
    private Image cardImage;

    private TMP_Text cardName;

    public Guid Identifier => cardData.Identifier;


    public CardInfo cardData;
    private Button button;
    private bool Istapped = false;
    private ZoneType currentzone;

    public void Setup(CardInfo card, Action<CardInfo> onClick = null)
    {
        cardData = card;

        if (cardName != null)
        {
            cardName.text = card.Name;
        }

        button = GetComponent<Button>();
        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
        }

        button.onClick.RemoveAllListeners();
        

        if (onClick != null)
        {
            button.onClick.AddListener(() => onClick(cardData));
        }

        if (!string.IsNullOrEmpty(card.ImageUri))
            StartCoroutine(APIManager.Instance.LoadImage(card.ImageUri, cardImage));

        // SignalRClient.Instance.OnMoveCardEvent += (_, e) =>
        // {
        //     if (cardData.Identifier != e.Identifier)
        //         return;
        //     if (e.Position.HasValue)
        //         transform.position = e.Position.Value.ToUnity3();
        // };
    }

    public void SetZones(ZoneType zone)
    {
        currentzone = zone;
    }


    

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (currentzone != ZoneType.Bf)
            {
                return;
            }

            Debug.Log("Q is pressed");
            transform.Rotate(0, 0, Istapped ? 90.0f : -90.0f);
            Istapped = !Istapped;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            CardMenu.Instance.Open(this);
        }
    }
}