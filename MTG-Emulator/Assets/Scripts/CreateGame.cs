using MTG_Emulator.UI;
using MTG_Emulator.Unity.Db.DTO.GameDTO;
using MTG_Emulator.Unity.Synchronization.Events;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    [SerializeField]
    private ToggleableButton toggleCodeButton;
    
    private bool unityIsStupid = false;

    private void Start()
    {
        maxPlayersSlider.minValue = 2;
        maxPlayersSlider.maxValue = 5;
        maxPlayersSlider.wholeNumbers = true;
        maxPlayersSlider.value = 4;
        updateSliderLabel(4);

        maxPlayersSlider.onValueChanged.AddListener(updateSliderLabel);
        createButton.onClick.AddListener(OnClickCreate);
        toggleCodeButton.Button.onClick.AddListener(refreshCode);
        
        refreshCode();
        setStatus("");

        if (SignalRClient.Instance == null)
        {
            Debug.Log("SignalR is null");
            return;
        }

        SignalRClient.Instance.OnTurnOrderCreatedEvent += HandleTurnOrderCreated;
    }

    private void Update()
    {
        // Scene loading has to happen on unity's main thread,
        // if this does not happen it will simply silently fail and refuse loading.
        // Since we want to essentially do it from an event response
        // (which would run on a background thread)
        // we have to use an incredibly stupid workaround like this:
        if (unityIsStupid)
            SceneManager.LoadScene("InGame");
    }

    private void OnDestroy()
    {
        if (SignalRClient.Instance != null)
        {
            SignalRClient.Instance.OnTurnOrderCreatedEvent -= HandleTurnOrderCreated;
        }

        maxPlayersSlider.onValueChanged.RemoveListener(updateSliderLabel);
        createButton.onClick.RemoveListener(OnClickCreate);
        toggleCodeButton.Button.onClick.RemoveListener(refreshCode);
    }

    private void refreshCode() => gameCodeText.text = toggleCodeButton.State ? GameSession.GameCode : "******";

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
            MaxPlayers = (int)maxPlayersSlider.value
        };

        StartCoroutine(APIManager.Instance.CreateGame(dto, onSuccess: response =>
            {
                GameSession.GameCode = response.GameCode;
                GameSession.MaxPlayers = response.MaxPlayers;
                GameSession.IsHost = true;
                GameSession.PlayerId = response.CurrentPlayers - 1;

                refreshCode();
                setStatus($"Game Created! Code:\n{response.GameCode}");
                Debug.Log($"[CreateGame] Room {response.GameCode} ready for {response.MaxPlayers} players.");
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


    private void HandleTurnOrderCreated(object sender, TurnOrderEvent e) => unityIsStupid = true;
}