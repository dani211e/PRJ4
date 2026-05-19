using System;
using System.Linq;
using System.Runtime.Serialization;
using MTG_Emulator.Cards;
using MTG_Emulator.Cards.Extensions;
using MTG_Emulator.Unity.Synchronization.Events;
using UnityEngine;

namespace MTG_Emulator
{
    public class CardListener : MonoBehaviour
    {
        private SignalRClient signalRClient;
        private CardManager cardManager;

        [SerializeField]
        private GameObject cardPrefab;

        [SerializeField]
        private Transform handZone;

        private void Awake()
        {
            signalRClient = FindFirstObjectByType<SignalRClient>();
            signalRClient.OnNewCardEvent += spawnNewCard;
        }

        private void OnDestroy()
        {
            signalRClient.OnNewCardEvent -= spawnNewCard;
        }

        private void spawnNewCard(object _, NewCardEvent e)
        {
            if (e.Card == null)
                return;
            var obj = Instantiate(cardPrefab, handZone);

            //Why is there no remove component ?!
            Destroy(obj.GetComponent<Drag>());

            var cardInfo = new CardInfo
            {
                Identifier = e.Identifier,
                ScryfallId = e.Card.ScryfallId,
                Name = e.Card.Name,
                ImageUri = e.Card.ImageUri,
                AltFace = e.Card.AltFace.ToCardInfo(),
                RelatedCards = e.Card.RelatedCards.Select(rc => rc.ToCardInfo()).ToList()
            };
            var c = obj.GetComponent<Card>();

            c.Setup(cardInfo);
            CardManager.AddObject(e.PlayerIndex, cardInfo, obj);
        }

    }
}