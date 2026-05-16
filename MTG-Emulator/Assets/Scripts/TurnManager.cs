using System.Collections.Generic;
using MTG_Emulator.Unity.Synchronization.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MTG_Emulator.Backend.DB.Models
{
    public class TurnManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text turnText;
        [SerializeField] private Button endTurnButton;

        [SerializeField] private List<string> players = new List<string>();

        private string localPlayerName;
        private string currentPlayerTurn;
        private int currentPlayerIndex = 0;

        private void Start()
        {
            localPlayerName = PlayerPrefs.GetString("username");

            if (SignalRClient.Instance == null)
            {
                Debug.LogError("SignalRClient.Instance is null");
                return;
            }

            SignalRClient.Instance.OnTurnChangedEvent += HandleTurnChanged;

            if (players.Count == 0)
            {
                Debug.LogError("No players added to TurnManager.");
                return;
            }

            currentPlayerTurn = players[0];
            UpdateTurnUI();
        }

        private void OnDestroy()
        {
            if (SignalRClient.Instance == null)
            {
                return;
            }

            SignalRClient.Instance.OnTurnChangedEvent -= HandleTurnChanged;
        }

        public void EndTurnOnClick()
        {
            if (!IsMyTurn())
            {
                Debug.Log("Not your turn.");
                return;
            }

            currentPlayerIndex++;

            if (currentPlayerIndex >= players.Count)
            {
                currentPlayerIndex = 0;
            }

            string nextPlayer = players[currentPlayerIndex];

            TurnChangedEvent turnEvent = new TurnChangedEvent
            {
                currentPlayerName = nextPlayer,
                turnNumber = currentPlayerIndex
            };

            SignalRClient.Instance.Broadcast(turnEvent);
            
        }

        private void HandleTurnChanged(object sender, TurnChangedEvent e)
        {
            currentPlayerTurn = e.currentPlayerName;
            currentPlayerIndex = e.turnNumber;

            UpdateTurnUI();
        }

        private void UpdateTurnUI()
        {
            turnText.text = "Turn: " + currentPlayerTurn;

            bool isMyTurn = IsMyTurn();
            endTurnButton.interactable = isMyTurn;

            Debug.Log(isMyTurn ? "It is my turn" : "Waiting for " + currentPlayerTurn);
        }

        public bool IsMyTurn()
        {
            return currentPlayerTurn == localPlayerName;
        }
    }
}