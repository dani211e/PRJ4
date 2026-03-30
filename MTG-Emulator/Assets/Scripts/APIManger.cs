using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class CreateDeckDto
{
    public string DeckName;
    public string PlayerName;
    public string CardList;
    public string Commander;
}

[Serializable]
public class CardDto
{
    public int CardId;
    public string Name;
    public string OracleText;
    public string ImageUri;
}

[Serializable]
public class DeckDto
{
    public string DeckName;
    public string DeckCommander;
    public System.Collections.Generic.List<CardDto> Cards;
}

public class APIManager : MonoBehaviour
{
    public static APIManager Instance;
    private string baseUrl = "http://localhost:5000/api/";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public IEnumerator CreateDeck(CreateDeckDto deck, Action<DeckDto> onSuccess, Action<string> onError)
    {
        string json = JsonUtility.ToJson(deck);
        UnityWebRequest request = new UnityWebRequest(baseUrl + "Deck", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(request.downloadHandler.text);
        }
        else
        {
            DeckDto result = JsonUtility.FromJson<DeckDto>(request.downloadHandler.text);
            onSuccess?.Invoke(result);
        }
    }

    public IEnumerator UpdateDeckCommander(string deckName, string commanderName, Action<DeckDto> onSuccess, Action<string> onError)
    {
        CreateDeckDto dto = new CreateDeckDto { DeckName = deckName, Commander = commanderName };
        string json = JsonUtility.ToJson(dto);

        UnityWebRequest request = new UnityWebRequest(baseUrl + $"Deck/{deckName}", "PUT");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
            onError?.Invoke(request.downloadHandler.text);
        else
            onSuccess?.Invoke(JsonUtility.FromJson<DeckDto>(request.downloadHandler.text));
    }
}
