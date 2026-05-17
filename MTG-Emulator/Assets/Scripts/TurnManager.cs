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

        private List<string> players = new();

        private string localPlayerName;
        private string currentPlayerTurn;
        private int currentPlayerIndex = 0;
        private int currentPhase = 0;


        private void Start()
        {
            localPlayerName = PlayerPrefs.GetString("username");

            if (SignalRClient.Instance == null)
            {
                Debug.LogError("SignalRClient.Instance is null");
                return;
            }

            SignalRClient.Instance.OnTurnChangedEvent += HandleTurnChanged;
            SignalRClient.Instance.OnTurnOrderCreatedEvent += HandleTurnOrderCreated;
            
            turnText.text = "waiting for players";
            endTurnButton.interactable = false;
        }

        private void OnDestroy()
        {
            if (SignalRClient.Instance == null)
            {
                return;
            }

            SignalRClient.Instance.OnTurnChangedEvent -= HandleTurnChanged;
            SignalRClient.Instance.OnTurnOrderCreatedEvent -= HandleTurnOrderCreated;

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
        
        private void HandleTurnOrderCreated(object sender, TurnOrderEvent e)
        {
            players = e.PlayersNames;
            currentPlayerTurn = e.currentPlayerName;
            currentPlayerIndex = players.IndexOf(currentPlayerTurn);

            UpdateTurnUI();
        }

        private void UpdateTurnUI()
        {
            turnText.text = "Turn: " + currentPlayerTurn;

            bool isMyTurn = IsMyTurn();
            endTurnButton.interactable = isMyTurn;

            Debug.Log(isMyTurn ? "It is my turn" : "Waiting for " + currentPlayerTurn);
        }

        public void NextPhaseOnClick()
        {
            if (!IsMyTurn())
            {
                return;
            }

            currentPhase++;

            if (currentPhase <= 2)
            {
                UpdateTurnUI();
                return;
            }

            currentPhase = 0;
            EndTurnToNextPlayer();
        }

        private void EndTurnToNextPlayer()
        {
            currentPhase++;

            if (currentPhase >= players.Count)
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
            HandleTurnChanged(this, turnEvent);
        }

        public bool IsMyTurn()
        {
            return currentPlayerTurn == localPlayerName;
        }
    }
}