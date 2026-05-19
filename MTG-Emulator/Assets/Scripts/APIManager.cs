using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using MTG_Emulator.Unity.Db.DTO.AuthenticationDTO;
using MTG_Emulator.Unity.Db.DTO.DeckDTO;
using MTG_Emulator.Unity.Db.DTO.GameDTO;
using MTG_Emulator.Unity.Db.DTO.PlayerDTO;
using UnityEngine;
using UnityEngine.Networking;
using JsonSerializer = System.Text.Json.JsonSerializer;
using System.Text.Json;


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
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public IEnumerator CreateDeck(CreateDeckDto deck, Action<DeckDto> onSuccess, Action<string> onError)
    {
        string json = JsonSerializer.Serialize(deck);
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
            DeckDto result = JsonSerializer.Deserialize<DeckDto>(request.downloadHandler.text);
            onSuccess?.Invoke(result);
        }

        Debug.Log(baseUrl + "Deck");
    }

    public IEnumerator UpdateDeckCommander(int deckId, List<string> commanderNames, string deckName, string cardList,
        Action<DeckDto> onSuccess, Action<string> onError)
    {
        var dto = new CreateDeckDto
        {
            DeckName = deckName,
            CardList = cardList,
            CommandZone = commanderNames
        };

        string json = JsonSerializer.Serialize(dto);

        UnityWebRequest request = new UnityWebRequest(baseUrl + $"Deck/{deckId}", "PUT");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        string token = PlayerPrefs.GetString("jwtToken");
        if (!string.IsNullOrEmpty(token))
            request.SetRequestHeader("Authorization", "Bearer " + token);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
            onError?.Invoke(request.downloadHandler.text);
        else
        {
            string responseText = request.downloadHandler.text;
            Debug.Log("UpdateDeckCommander response: " + responseText);

            if (string.IsNullOrEmpty(responseText))
            {
                Debug.Log("Commander updated successfully (empty response)");
                onSuccess?.Invoke(null);
            }
            else
                onSuccess?.Invoke(JsonSerializer.Deserialize<DeckDto>(responseText, JsonOptions));
        }
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
            onError?.Invoke(request.downloadHandler.text);
        }
        else
        {
            var reponse = JsonSerializer.Deserialize<LoginResponseDto>(reponseJson);

            PlayerPrefs.SetString("jwtToken", reponse.Token);
            PlayerPrefs.SetString("username", reponse.Username);
            PlayerPrefs.Save();

            GameSession.PlayerName = reponse.Username;

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
            Debug.LogError("Get failed: " + request.result + " | HTTP " + request.responseCode + " | " + request.downloadHandler.text);
            onError?.Invoke(request.result + " | HTTP " + request.responseCode + " | " + request.downloadHandler.text);
        }
        else
        {
            Debug.Log("RAW GET RESPONSE: " + request.downloadHandler.text);

            var result = JsonSerializer.Deserialize<List<DeckDto>>(request.downloadHandler.text);

            onSuccess?.Invoke(result);
        }
    }

    public IEnumerator GetDeckById(int deckId, Action<DeckDto> onSuccess, Action<string> onError)
    {
        string uri = baseUrl + "Deck/" + UnityWebRequest.EscapeURL(deckId.ToString());
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

            var result = JsonSerializer.Deserialize<DeckDto>(request.downloadHandler.text);

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
namespace MTG_Emulator.Unity.Db.DTO.DeckDTO
{
    public class UpdateDeckDto
    {
        public List<string> CommandZone { get; set; } = new();
    }
}