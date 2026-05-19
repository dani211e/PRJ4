using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using MTG_Emulator.Backend.DB.Models;
using MTG_Emulator.Unity.Db.DTO.CardDTO;
using MTG_Emulator.Unity.Db.DTO.RelatedCardDTO;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class Token : MonoBehaviour
{
    [Header("Visible UI")] [SerializeField]
    private Image tokenImage;
    
    private TMP_Text cardName;


    private RelatedCardDto tokenData;
    private Card card;
    private Button button;
    private bool Istapped = false;
    private CardZonesTypes currentzone;

    public void Setup(RelatedCardDto token, Action<CardDto> onClick = null)
    {
        if (!string.IsNullOrEmpty(token.ImageUri))
        {
            string fullImageUrl = "http://localhost:5042" + token.ImageUri;
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
            tokenImage.sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f)
            );
        }
    }
}