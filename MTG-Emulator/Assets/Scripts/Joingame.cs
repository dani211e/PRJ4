using MTG_Emulator.Unity.Db.DTO.GameDTO;
using MTG_Emulator.Unity.Synchronization.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class JoinGame : MonoBehaviour
{
    [Header ("UI References")]
    [SerializeField] private TMP_InputField codeInputField;

    [SerializeField] private Button joinButton;
    
    [SerializeField] private TMP_Text statusText;

    private void Start()
    {
        codeInputField.characterLimit = 6;
        codeInputField.onValueChanged.AddListener(OnCodeChanged);
        
        joinButton.interactable = false;
        joinButton.onClick.AddListener(OnClickJoin);
    }
    private void OnDestroy()
    {
        codeInputField.onValueChanged.RemoveListener(OnCodeChanged);
        joinButton.onClick.RemoveListener(OnClickJoin);
    }   

    private void OnCodeChanged(string value)
    {
        string upper  = value.ToUpper();
        if (value != upper)
        {
            codeInputField.SetTextWithoutNotify(upper);
            codeInputField.caretPosition = upper.Length;
        }

        joinButton.interactable = upper.Length == 6;
        SetStatus(upper.Length != 6 ? "Press Join!" : $"{upper.Length}/6 characters");
    }

    public void OnClickJoin()
    {
        string code = codeInputField.text.Trim().ToUpper();

        if (code.Length != 6)
        {
            SetStatus("Please enter a valid 6-character code.");
            return;
        }
        
        if (APIManager.Instance == null)
        {
            Debug.LogError("APIManager instance is null");
            return;
        }
        
        joinButton.interactable = false;
        SetStatus("Joining...");

        var dto = new JoinGameDto
        {
            GameCode = code,
            PlayerName = GameSession.PlayerName
        };
        
        StartCoroutine(APIManager.Instance.JoinGame(dto, onSuccess: response =>
        {
            GameSession.GameCode = response.GameCode;
            GameSession.MaxPlayers = response.MaxPlayers;
            GameSession.IsHost = false;
            GameSession.PlayerId = response.CurrentPlayers - 1;
            
            SetStatus($"Joined! ({response.CurrentPlayers}/{response.MaxPlayers} players)");
            Debug.Log($"[JoinGame] Joined room {response.GameCode}");


            if (response.CurrentPlayers == response.MaxPlayers)
            {
                TurnOrderEvent turnOrderEvent = new TurnOrderEvent
                {
                    PlayersNames = response.PlayerNames,
                    CurrentPlayerName = response.CurrentPlayerName,
                };

                if (SignalRClient.Instance == null)
                {
                    return;
                }
                
                SignalRClient.Instance.Broadcast(turnOrderEvent);
                SceneManager.LoadScene("InGame");
            }
        },
        onError: error =>
        {
            joinButton.interactable = true;
            SetStatus($"Failed: {error}");
            Debug.LogError($"[JoinGame] {error}");
        } ));

    }
    public void OnClickBack()
    {
        SceneManager.LoadScene("0");
    }
    private void SetStatus(string msg)
    {
        if (statusText != null)
            statusText.text = msg;
    }
}