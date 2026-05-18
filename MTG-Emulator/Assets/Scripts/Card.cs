using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using MTG_Emulator.Backend.DB.Models;
using MTG_Emulator.Unity.Db.DTO.CardDTO;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class Card : MonoBehaviour
{
    [Header("Visible UI")]
    [SerializeField] private Image cardImage;
    private TMP_Text cardName;


    private CardDto cardData;
    private Button button;
    private bool Istapped = false;
    private CardZonesTypes currentzone;

    public void Setup(CardDto card, Action<CardDto> onClick = null)
    {
        cardData = card;

        if (cardName != null)
        {
            cardName.text = card.Name;
        }

        if (!string.IsNullOrEmpty(card.ImageUri))
        {
            string fullImageUrl = "http://localhost:5042" + card.ImageUri;
            StartCoroutine(LoadCardImage(fullImageUrl));
        }
    }

    public void SetZones(CardZonesTypes zone)
    {
        currentzone = zone;
    }
    

    private IEnumerator LoadCardImage(string url)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
            Debug.LogError("Failed to load card image: " + url);
        else
        {
            Texture2D tex = DownloadHandlerTexture.GetContent(request);
            cardImage.sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f)
            );
        }
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