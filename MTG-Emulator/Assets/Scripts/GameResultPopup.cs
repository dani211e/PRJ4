using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class GameResultPopup : MonoBehaviour
    {
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
            
            
            Debug.Log("Leaving game");
        }
    }
}