using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using MTG_Emulator.Backend.DB.Models;
using MTG_Emulator.Unity.Db.DTO.CardDTO;
using MTG_Emulator.Unity.Db.DTO.RelatedCardDTO;
using MTG_Emulator.Unity.Synchronization.Enums;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class TokenManager : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform battlefieldParent;
    private TMP_Text cardName;


    private CardDto cardData;
    private Button button;
    private bool Istapped = false;
    private ZoneType currentzone;
    /*private void OnGUI()
    {
        Event rightClick =  Event.current;
        if (rightClick.button == 1 && rightClick.isMouse)
        {
            Card card = 
            RelatedCardDto token = card.RelatedCards[0];
            GameObject tokenObj = Instantiate(cardPrefab, battlefieldParent);
            Token tokenScript = tokenObj.GetComponent<Token>();
            tokenScript.Setup(token);
            Debug.Log(token.Name + "Has been summoned");
        }
    }*/
}