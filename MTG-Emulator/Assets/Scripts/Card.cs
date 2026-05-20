using System;
using System.Collections;
using MTG_Emulator.Cards;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [Header("Visible UI")]
    [SerializeField]
    private Image cardImage;

    private TMP_Text cardName;

    public Guid Identifier => cardData.Identifier;

    private CardInfo cardData;
    private Button button;
    private bool Istapped = false;
    private CardZonesTypes currentzone;

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
        {
            string fullImageUrl = "http://localhost:5042" + card.ImageUri;
            StartCoroutine(APIManager.Instance.LoadImage(fullImageUrl, cardImage));
        }

        // SignalRClient.Instance.OnMoveCardEvent += (_, e) =>
        // {
        //     if (cardData.Identifier != e.Identifier)
        //         return;
        //     if (e.Position.HasValue)
        //         transform.position = e.Position.Value.ToUnity3();
        // };
    }

    public void SetZones(CardZonesTypes zone)
    {
        currentzone = zone;
    }


    

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (currentzone != CardZonesTypes.Bf)
            {
                return;
            }

            Debug.Log("Q is pressed");
            transform.Rotate(0, 0, Istapped ? 90.0f : -90.0f);
            Istapped = !Istapped;
        }
    }
}