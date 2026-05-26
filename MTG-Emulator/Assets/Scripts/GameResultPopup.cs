using System;
using MTG_Emulator.Unity.Synchronization.Events;
using UnityEngine;
using MTG_Emulator.Threading;

namespace DefaultNamespace
{
    public class GameResultPopup : MonoBehaviour
    {
        public void OnClickReportResult(int gameResult)
        {
            Debug.Log(gameResult);
            
            StartCoroutine(APIManager.Instance.UpdatePlayerStats(gameResult,
                s => {  }, 
                error => { Debug.LogError("Failed to update player stats " + error); }));
            
            LeaveGame();
        }

        public void LeaveGame()
        {
            SignalRClient.Instance.Broadcast(new PlayerLeaveEvent(GameSession.PlayerId));
            
            StartCoroutine(APIManager.Instance.LeaveGame(GameSession.GameCode,
                () => { Debug.Log("Leaving game"); },
                error => { Debug.LogError("Failed to leave game: " + error); }));
        }
    }
}