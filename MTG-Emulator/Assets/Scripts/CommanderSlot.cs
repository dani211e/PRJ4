using System.Collections.Generic;
using MTG_Emulator.Cards;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;

public class CommanderSlot : MonoBehaviour
{
    [SerializeField] private GameObject commanderCardPrefab;
    [SerializeField] private Transform slotParent;

    public void PlaceCommander(List<CardInfo> commanders)
    {
        foreach(Transform child in slotParent)
        Destroy(child.gameObject);

        foreach (var commander in commanders)
        {
            GameObject cardObj = Instantiate(commanderCardPrefab, slotParent);
            
            Card cardScript = cardObj.GetComponent<Card>();
            if(cardScript != null)
                    cardScript.Setup(commander);
            
            Image img = cardObj.GetComponentInChildren<Image>();
            if(img != null && !string.IsNullOrEmpty(commander.ImageUri))
                StartCoroutine(APIManager.Instance.LoadImage(commander.ImageUri, img));
        }
        Debug.Log($"Placed {commanders.Count} commander(s) in the command zone.");
    }
}