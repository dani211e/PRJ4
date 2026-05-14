using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using MTG_Emulator.Unity.Db.DTO.AuthenticationDTO;
using MTG_Emulator.Unity.Db.DTO.GameDTO;
using MTG_Emulator.Unity.Db.DTO.PlayerDTO;
using UnityEngine;
using UnityEngine.Networking;
using JsonSerializer = System.Text.Json.JsonSerializer;

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

        string token = PlayerPrefs.GetString("jwtToken");

        if (!string.IsNullOrEmpty(token))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
        }

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

    public IEnumerator UpdateDeckCommander(string deckName, string commanderName, Action<DeckDto> onSuccess,
        Action<string> onError)
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

    public IEnumerator Login(string email, string password, Action<string> onSuccess, Action<string> onError)
    {
        var loginData = new LoginDto { Email = email, Password = password };

        string json = JsonSerializer.Serialize(loginData);
        string url = baseUrl + "Authentication/login";
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.SetRequestHeader("Content-Type", "application/json");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        string reponseJson = request.downloadHandler.text;


        if (request.result != UnityWebRequest.Result.Success)
        {
            var reponse = JsonSerializer.Deserialize<LoginResponseDto>(reponseJson);

            PlayerPrefs.SetString("jwtToken", reponse.Token);
            PlayerPrefs.SetString("username", reponse.Username);
            PlayerPrefs.Save();

            onError?.Invoke(request.downloadHandler.text);
        }
        else
        {
            onSuccess?.Invoke(request.downloadHandler.text);
        }
    }


    public IEnumerator GetDecksByUsername(string username, Action<List<DeckDto>> onSuccess, Action<string> onError)
    {
        string uri = baseUrl + "Deck/player/" + UnityWebRequest.EscapeURL(username);
        UnityWebRequest request = new UnityWebRequest(uri, "GET");
        request.downloadHandler = new DownloadHandlerBuffer();

        string token = PlayerPrefs.GetString("jwtToken");

        if (!string.IsNullOrEmpty(token))
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
        }

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Get failed" + request.downloadHandler.text);
            onError?.Invoke(request.downloadHandler.text);
        }
        else
        {
            Debug.Log("RAW GET RESPONSE: " + request.downloadHandler.text);

            var result = JsonSerializer.Deserialize<List<DeckDto>>(request.downloadHandler.text);

            onSuccess?.Invoke(result);
        }
    }

    public IEnumerator CreateGame(CreateGameDto dto, Action<GameResponseDto> onSuccess, Action<string> onError)
    {
        string json = JsonSerializer.Serialize(dto);
        UnityWebRequest request = new UnityWebRequest(baseUrl + "Game", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
            onError?.Invoke(request.downloadHandler.text);
        else
            onSuccess?.Invoke(JsonSerializer.Deserialize<GameResponseDto>(request.downloadHandler.text));
    }

    public IEnumerator JoinGame(JoinGameDto dto, Action<GameResponseDto> onSuccess, Action<string> onError)
    {
        string json = JsonSerializer.Serialize(dto);
        UnityWebRequest request = new UnityWebRequest(baseUrl + "Game/join", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
            onError?.Invoke(request.downloadHandler.text);
        else
            onSuccess?.Invoke(JsonSerializer.Deserialize<GameResponseDto>(request.downloadHandler.text));
    }
}