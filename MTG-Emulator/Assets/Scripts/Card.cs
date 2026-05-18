using System;
using System.Collections;
using MTG_Emulator;
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


    private CardInfo cardData;
    private Button button;
    private bool Istapped = false;
    private CardZonesTypes currentzone;

    public void Setup(CardInfo card, Action<CardInfo> onClick = null)
    {
        cardData = card;
        ObjectManager.AddObject(GameSession.PlayerId, cardData.Identifier, gameObject);

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