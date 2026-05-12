using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Networking;

public class Card : MonoBehaviour
{
    [Header("Visible UI")]
    [SerializeField] private Image cardImage;
    private TMP_Text cardName;


    private CardDto cardData;
    private Button button;

    public void Setup(CardDto card, Action<CardDto> onClick = null)
    {
        cardData = card;

        if (cardName != null)
            cardName.text = card.name;

        button = GetComponent<Button>();
        if (button == null)
            button = gameObject.AddComponent<Button>();

        button.onClick.RemoveAllListeners();

        if (onClick != null)
            button.onClick.AddListener(() => onClick(cardData));

        if (!string.IsNullOrEmpty(card.imageUri))
        {
            string fullImageUrl = "http://localhost:5042" + card.imageUri;
            StartCoroutine(LoadCardImage(fullImageUrl));
        }
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
}