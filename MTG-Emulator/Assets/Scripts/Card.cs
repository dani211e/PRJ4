using System;
using MTG_Emulator.Cards;
using MTG_Emulator.Unity.Synchronization.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
    private Tapable tapable;
    public ZoneType CurrentZone { get; private set; }

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
        tapable = GetComponent<Tapable>();
        if (tapable == null)
            tapable = gameObject.AddComponent<Tapable>();

        tapable.SetIdentifier(card.Identifier);
    }

    public void SetZones(ZoneType zone)
    {
        CurrentZone = zone;
        Debug.Log($"SetZones called: {zone}, tapable is null: {tapable == null}");
        tapable?.SetZone(zone);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            CardMenu.Instance.Open(this);
        }
    }
}