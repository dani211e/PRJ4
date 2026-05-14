using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using MTG_Emulator.Unity.Db.DTO.GameDTO;
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
    public int cardId;
    public string name;
    public string oracleText;
    public string imageUri;
}

[Serializable]
public class DeckDto
{
    public string deckName;
    public string deckCommander;
    public List<CardDto> cards;
}

public class APIManager : MonoBehaviour
{
    public static APIManager Instance;
    private string baseUrl = "http://localhost:5042/api/";

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
        Debug.Log(baseUrl + "Deck");
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

    public IEnumerator CreateProfile(string playerName, string password, Action<string> onSuccess, Action<string> onError)
    {
        string url = baseUrl + "Player?playerName=" +
                     UnityWebRequest.EscapeURL(playerName) +
                     "&password=" +
                     UnityWebRequest.EscapeURL(password);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(request.downloadHandler.text);
        }
        else
        {
            onSuccess?.Invoke(request.downloadHandler.text);
        }
    }



    public IEnumerator GetDeckByName(string deckname, Action<DeckDto> onSuccess, Action<string> onError)
    {
        string uri = baseUrl + "Deck/" + UnityWebRequest.EscapeURL(deckname);
        UnityWebRequest request = new UnityWebRequest(uri, "GET");
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            onError?.Invoke(request.downloadHandler.text);
        }
        else
        {
            Debug.Log("RAW GET RESPONSE: " + request.downloadHandler.text);

            DeckDto result = JsonUtility.FromJson<DeckDto>(request.downloadHandler.text);

            Debug.Log("Parsed deck name: " + result.deckName);
            Debug.Log("Parsed commander: " + result.deckCommander);
            Debug.Log("Parsed cards count: " + (result.cards == null ? -1 : result.cards.Count));

            onSuccess?.Invoke(result);
        }
    }
    public IEnumerator CreateGame(CreateGameDto dto, Action<GameResponseDto> onSuccess, Action<string> onError)
    {
        string json = JsonUtility.ToJson(dto);
        UnityWebRequest request = new UnityWebRequest(baseUrl + "Game", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler   = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
            onError?.Invoke(request.downloadHandler.text);
        else
            onSuccess?.Invoke(JsonUtility.FromJson<GameResponseDto>(request.downloadHandler.text));
    }

    public IEnumerator JoinGame(JoinGameDto dto, Action<GameResponseDto> onSuccess, Action<string> onError)
    {
        string json = JsonUtility.ToJson(dto);
        UnityWebRequest request = new UnityWebRequest(baseUrl + "Game/join", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler   = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
            onError?.Invoke(request.downloadHandler.text);
        else
            onSuccess?.Invoke(JsonUtility.FromJson<GameResponseDto>(request.downloadHandler.text));
    }
    
}
