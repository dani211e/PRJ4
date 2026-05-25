using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using MTG_Emulator.Backend.DB.Models;
using MTG_Emulator.Cards;
using MTG_Emulator.Unity.Db.DTO.CardDTO;
using MTG_Emulator.Unity.Db.DTO.RelatedCardDTO;
using MTG_Emulator.Unity.Synchronization.Enums;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class Token : MonoBehaviour, IPointerClickHandler
{
    [Header("Visible UI")] [SerializeField]
    private Image tokenImage;
    
    private TMP_Text cardName;


    private RelatedCardDto tokenData;
    private Card card;
    private Button button;
    private Tapable tapable;
    private ZoneType currentzone;

    public void Setup(RelatedCardInfo token, Action<CardDto> onClick = null)
    {
        if (!string.IsNullOrEmpty(token.ImageUri))
        {
            StartCoroutine(APIManager.Instance.LoadImage(token.ImageUri, tokenImage));
        }
        tapable = GetComponent<Tapable>();
        if (tapable == null)
            {
            tapable = GetComponent<Tapable>();
            }
    }

    public void SetZones(ZoneType zone)
    {
        currentzone = zone;
        tapable.SetZone(zone);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            CardMenu.Instance.Open(this);
        }
    }
}