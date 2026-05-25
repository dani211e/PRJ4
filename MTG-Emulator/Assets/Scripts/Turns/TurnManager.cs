using System.Collections.Generic;
using MTG_Emulator.Unity.Synchronization.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MTG_Emulator.Threading;

namespace MTG_Emulator.Turns
{
    public class TurnManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text turnText;
        [SerializeField] private Button endTurnButton;

        private List<string> players = new();

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

            
            SignalRClient.Instance.OnTurnChangedEvent += handleTurnChanged;
            SignalRClient.Instance.OnTurnOrderCreatedEvent += handleTurnOrderCreated;
            
            turnText.text = "waiting for players";
            endTurnButton.interactable = false;

            if (SignalRClient.Instance.LatestTurnOrder != null)
            {
                handleTurnOrderCreated(null, SignalRClient.Instance.LatestTurnOrder);
            } else if (SignalRClient.Instance.LatestTurnChanged != null)
            {
                handleTurnChanged(null, SignalRClient.Instance.LatestTurnChanged);
            }
        }

        private void OnDestroy()
        {
            if (SignalRClient.Instance == null)
            {
                return;
            }

            SignalRClient.Instance.OnTurnChangedEvent -= handleTurnChanged;
            SignalRClient.Instance.OnTurnOrderCreatedEvent -= handleTurnOrderCreated;

        }
        

        private void handleTurnChanged(object sender, TurnChangedEvent e)
        {
                currentPlayerTurn = e.currentPlayerName;
                currentPlayerIndex = e.turnNumber;

                updateTurnUI();
        }
        
        private void handleTurnOrderCreated(object sender, TurnOrderEvent e)
        {
                players = e.PlayersNames;
                currentPlayerTurn = e.CurrentPlayerName;
                currentPlayerIndex = players.IndexOf(currentPlayerTurn);
    
                updateTurnUI();
        }

        public void EndTurnToNextPlayer()
        {

            if (!IsMyTurn())
            {
                Debug.Log("not your turn");
                return;
            }

            if (players.Count == 0)
            {
                return;
            }
            
            int nextIndex = (currentPlayerIndex + 1) % players.Count;
            string nextPlayer = players[nextIndex];

            
            SignalRClient.Instance.Broadcast(new TurnChangedEvent
            {
                currentPlayerName = nextPlayer,
                turnNumber = nextIndex,
            });
        }

        public bool IsMyTurn()
        {
            return currentPlayerTurn == localPlayerName;
        }
        
        private void updateTurnUI()
        {
            turnText.text = "Turn: " + currentPlayerTurn;

            bool isMyTurn = IsMyTurn();
            endTurnButton.interactable = isMyTurn;

            Debug.Log(isMyTurn ? "It is my turn" : "Waiting for " + currentPlayerTurn);
        }
    }
}