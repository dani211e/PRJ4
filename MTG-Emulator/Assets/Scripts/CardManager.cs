using System;
using System.Collections.Generic;
using System.Linq;
using MTG_Emulator.Cards;
using MTG_Emulator.Extensions;
using MTG_Emulator.Unity.Db.DTO.CardDTO;
using MTG_Emulator.Unity.Db.DTO.CardFaceDTO;
using MTG_Emulator.Unity.Db.DTO.RelatedCardDTO;
using MTG_Emulator.Unity.Synchronization.Events;
using UnityEngine;

namespace MTG_Emulator
{
    public class CardManager : MonoBehaviour
    {
        // An array of dictionaries where each index represents a player in the game,
        // the key is an object's identifier (e.g Card.Identifier) which is received from events
        private static Dictionary<Guid, GameObject>[] cards;

        public void Awake()
        {
            // Krilling my shelf for making this abomination of an initializer
            cards = new Dictionary<Guid, GameObject>[GameSession.MaxPlayers]
                .Select(_ => new Dictionary<Guid, GameObject>()).ToArray();
        }

        public static void AddObject(int playerIndex, CardInfo card, GameObject obj)
        {
            if (!cards[playerIndex].TryAdd(card.Identifier, obj))
                return;

            if (playerIndex != GameSession.PlayerId)
                return;

            SignalRClient.Instance.Broadcast(new NewCardEvent(playerIndex, card.Identifier)
            {
                Position = obj.transform.position.ToSystem2(),
                Card = new CardDto
                {
                    ScryfallId = card.ScryfallId,
                    Name = card.Name,
                    ImageUri = card.ImageUri,
                    AltFace = card.AltFace == null
                        ? null
                        : new CardFaceDto
                        {
                            Name = card.AltFace.Name,
                            ImageUri = card.AltFace.ImageUri
                        },
                    RelatedCards = card.RelatedCards.Select(rc => new RelatedCardDto
                    {
                        Name = rc.Name,
                        ImageUri = rc.ImageUri,
                    }).ToList(),
                }
            });
            Debug.Log("Sent new card event");
        }

        public static GameObject Get(int playerIndex, Guid identifier) =>
            cards[playerIndex].GetValueOrDefault(identifier);
    }
}