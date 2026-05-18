using System;
using System.Collections.Generic;
using System.Linq;
using MTG_Emulator.Unity.Synchronization.Events;
using UnityEngine;

namespace MTG_Emulator
{
    public class CardManager : MonoBehaviour
    {
        // An array of dictionaries where each index represents a player in the game,
        // the key is an object's identifier (e.g Card.Identifier) which is received from events
        private static Dictionary<Guid, GameObject>[] cards;

        [SerializeField]
        private GameObject cardPrefab;

        public void Awake()
        {
            // Krilling my shelf for making this abomination of an initializer
            cards = new Dictionary<Guid, GameObject>[GameSession.MaxPlayers]
                .Select(_ => new Dictionary<Guid, GameObject>()).ToArray();
        }

        public static void AddObject(int playerIndex, Guid identifier, GameObject obj)
        {
            if (!cards[playerIndex].TryAdd(identifier, obj))
                return;

            if (playerIndex != GameSession.PlayerId)
                return;

            SignalRClient.Instance.Broadcast(new NewCardEvent(playerIndex, identifier)
            {
                Position = obj.transform.position.ToSystem2(),
            });
            Debug.Log("Sent new card event");
        }

        public static GameObject Get(int playerIndex, Guid identifier) =>
            cards[playerIndex].GetValueOrDefault(identifier);
    }
}