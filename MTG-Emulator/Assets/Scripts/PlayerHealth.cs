using System;
using MTG_Emulator.Unity.Synchronization.Events;
using UnityEngine;
using TMPro;

namespace MTG_Emulator.Backend.DB.Models
{
    public class PlayerHealth : MonoBehaviour
    {
        
        private string playerName;
        [SerializeField] private TMP_Text healthText;

        private int health = 40;

        private void Start()
        {
            healthText.text = health.ToString();

            if (SignalRClient.Instance != null)
            {
                SignalRClient.Instance.OnPlayerStatsEvent += HandlePlayersStatsChanged;
            }
        }

        private void OnDestroy()
        {
            if (SignalRClient.Instance != null)
            {
                SignalRClient.Instance.OnPlayerStatsEvent -= HandlePlayersStatsChanged;
            }
        }

        public void SetUp(string ownerPlayerName)
        {
            playerName = ownerPlayerName;
            health = 40;
            healthText.text = health.ToString();
        } 
        
        public void AddHealth()
        {
            ChangeHealth(1);
        }

        public void RemoveHealth()
        {
            ChangeHealth(-1);
        }

        private void ChangeHealth(int amount)
        {
            health += amount;
            healthText.text = health.ToString();

            PlayerStatsEvent statsEvent = new PlayerStatsEvent(GameSession.PlayerId)
            {
                PlayerName = playerName,
                Health = health
            };
            
            SignalRClient.Instance.Broadcast(statsEvent);
        }


        private void HandlePlayersStatsChanged(object sender, PlayerStatsEvent e)
        {
            if (e.PlayerName != playerName)
            {
                return;
            }

            if (e.Health.HasValue)
            {
                health = e.Health.Value;
                healthText.text = health.ToString();
            }
        }
    }
}