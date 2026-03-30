using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Networking;

public class CardUI : MonoBehaviour
{
    public TMP_Text nameText;
    public Image cardImage;
    public Sprite backImage;

    private CardDto cardData;
    private Button button;

    public void Setup(CardDto card, Action<CardDto> onClick)
    {
        cardData = card;
        nameText.text = card.Name;

        button = GetComponent<Button>();
        if (button == null)
            button = gameObject.AddComponent<Button>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke(cardData));

        if (!string.IsNullOrEmpty(card.ImageUri))
            StartCoroutine(LoadCardImage(card.ImageUri));
        else
            cardImage.sprite = backImage;
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
            cardImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
    }
}
