using System.Text;
using MTG_Emulator.Unity.Db.DTO.GameDTO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CreateGame : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text  gameCodeText;
    [SerializeField] private Slider    maxPlayersSlider;
    [SerializeField] private TMP_Text  maxPlayersLabel;
    [SerializeField] private Button    createButton;
    [SerializeField] private TMP_Text  statusText;

    private string generatedCode;

    private void Start()
    {
        maxPlayersSlider.minValue = 2;
        maxPlayersSlider.maxValue = 5;
        maxPlayersSlider.wholeNumbers = true;
        maxPlayersSlider.value = 4;
        UpdateSliderLabel(4);

        maxPlayersSlider.onValueChanged.AddListener(UpdateSliderLabel);
        createButton.onClick.AddListener(OnClickCreate);

        RefreshCode();
        SetStatus("");
    }

    private void OnDestroy()
    {
        maxPlayersSlider.onValueChanged.RemoveListener(UpdateSliderLabel);
        createButton.onClick.RemoveListener(OnClickCreate);
    }

    public void RefreshCode()
    {
        generatedCode = GenerateCode(6);
        gameCodeText.text = generatedCode;
    }

    private void UpdateSliderLabel(float value)
    {
        maxPlayersLabel.text = $"{(int)value} Players";
    }

    public void OnClickCreate()
    {
        if (APIManager.Instance == null)
        {
            Debug.LogError("APIManager not found");
            return; 
        }
        
        createButton.interactable = false;
        statusText.text = "Creating Game...";

        var dto = new CreateGameDto
        {
            maxPlayers = (int)maxPlayersSlider.value,
            hostName = GameSession.PlayerName //login player name

        };
        
        StartCoroutine(APIManager.Instance.CreateGame(dto, onSuccess: response =>
        {
            GameSession.GameCode = response.gameCode;
            GameSession.MaxPlayers = response.maxPlayers;
            GameSession.IsHost = true;

            SetStatus($"Game Created! Code:{response.gameCode}");
            Debug.Log($"[CreateGame] Room {response.gameCode} ready for {response.maxPlayers} players.");
            
        },
            onError: error =>
            {
                createButton.interactable = true;
                SetStatus($"Error: {error}");
                Debug.LogError($"[CreateGame] Error: {error}");
            }));
        
    }

    public void OnClickBack()
    {
        SceneManager.LoadScene("0");
    }

    private void SetStatus(string status)
    {
        if (statusText != null)
            statusText.text = status;
    }
}

