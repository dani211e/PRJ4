using System;
using MTG_Emulator.Unity.Synchronization.Events;
using UnityEngine;

namespace DefaultNamespace
{
    public class GameResultPopup : MonoBehaviour
    {
        private void Start()
        {
            SignalRClient.Instance.OnPlayerLeaveEvent += handlePlayerLeaveEvent;
        }

        private void OnDestroy()
        {
            SignalRClient.Instance.OnPlayerLeaveEvent -= handlePlayerLeaveEvent;
        }

        public void OnClickReportResult(int gameResult)
        {
            Debug.Log(gameResult);
            StartCoroutine(APIManager.Instance.UpdatePlayerStats(gameResult,
                s => {}, 
                error => {Debug.LogError("Failed to update player stats " + error);}));
            
            LeaveGame();
        }

        public void LeaveGame()
        {
            SignalRClient.Instance.Broadcast(new PlayerLeaveEvent(GameSession.PlayerId));
            Debug.Log("Leaving game");
        }

        private void handlePlayerLeaveEvent(object sender, PlayerLeaveEvent e)
        {
            Debug.Log("HandlePlayerLeaveEvent fired");
        }
    }
}