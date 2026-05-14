using System.Text;
using MTG_Emulator.Unity.Db.DTO.GameDTO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CreateGame : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField]
    private TMP_Text gameCodeText;

    [SerializeField]
    private Slider maxPlayersSlider;

    [SerializeField]
    private TMP_Text maxPlayersLabel;

    [SerializeField]
    private Button createButton;

    [SerializeField]
    private TMP_Text statusText;
    
    [Header("Streamer Mode")]
    [SerializeField] private Button toggleCodeButton;
    [SerializeField] private TMP_Text toggleButtonText;
    
    private bool codeVisible = true;

    private void Start()
    {
        maxPlayersSlider.minValue = 2;
        maxPlayersSlider.maxValue = 5;
        maxPlayersSlider.wholeNumbers = true;
        maxPlayersSlider.value = 4;
        updateSliderLabel(4);

        maxPlayersSlider.onValueChanged.AddListener(updateSliderLabel);
        createButton.onClick.AddListener(OnClickCreate);
        toggleCodeButton.onClick.AddListener(OnClickToggleCode);

        refreshCode();
        setStatus("");
    }

    private void OnDestroy()
    {
        maxPlayersSlider.onValueChanged.RemoveListener(updateSliderLabel);
        createButton.onClick.RemoveListener(OnClickCreate);
        toggleCodeButton.onClick.RemoveListener(OnClickToggleCode);
    }

    private void refreshCode()
    {
        gameCodeText.text = GameSession.GameCode;
    }
    
    public void OnClickToggleCode()
    {
        codeVisible = !codeVisible;

        if (codeVisible)
        {
            gameCodeText.text = GameSession.GameCode;
            toggleButtonText.text = "HIDE CODE";
        }
        else
        {
            gameCodeText.text = "••••••";
            toggleButtonText.text = "SHOW CODE";
        }
    }


    private void updateSliderLabel(float value)
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
            MaxPlayers = (int)maxPlayersSlider.value,
            HostName = GameSession.PlayerName
        };

        StartCoroutine(APIManager.Instance.CreateGame(dto, onSuccess: response =>
            {
                GameSession.GameCode = response.gameCode;
                GameSession.MaxPlayers = response.maxPlayers;
                GameSession.IsHost = true;

                setStatus($"Game Created! Code:{response.gameCode}");
                Debug.Log($"[CreateGame] Room {response.gameCode} ready for {response.maxPlayers} players.");
            },
            onError: error =>
            {
                createButton.interactable = true;
                setStatus($"Error: {error}");
                Debug.LogError($"[CreateGame] Error: {error}");
            }));
    }

    public void OnClickBack()
    {
        SceneManager.LoadScene("0");
    }

    private void setStatus(string status)
    {
        if (statusText != null)
            statusText.text = status;
    }
}