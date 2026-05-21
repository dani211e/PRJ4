using MTG_Emulator.Cards.Extensions;
using MTG_Emulator.Extensions;
using MTG_Emulator.Threading;
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

        [SerializeField]
        private Transform battlefield;

        private void Awake()
        {
            signalRClient = FindFirstObjectByType<SignalRClient>();
            signalRClient.OnNewCardEvent += spawnNewCard;
            signalRClient.OnMoveCardEvent += moveCard;
        }

        private void OnDestroy()
        {
            signalRClient.OnNewCardEvent -= spawnNewCard;
            signalRClient.OnMoveCardEvent -= moveCard;
        }

        private void spawnNewCard(object _, NewCardEvent e)
        {
            MainThreadDispatcher.Enqueue(() =>
            {
                if (e.Card == null)
                    return;
                var obj = Instantiate(cardPrefab, handZone);

                obj.RemoveComponent<Drag>();

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
            });
        }

        private void moveCard(object _, MoveCardEvent e)
        {
            MainThreadDispatcher.Enqueue(() =>
            {
                Debug.Log($"move event rec {e.Identifier}");
                if (!e.Position.HasValue)
                    return;

                var c = CardManager.Get(e.PlayerIndex, e.Identifier);
                //this is wrong i cba, but we need to move the card from handzone to other zone somehow idk
                c.transform.parent = battlefield;

                var newPos = e.Position.Value.ToUnity3();
                c.transform.position = new Vector3(newPos.x, newPos.y - 100, 0);
            });
        }
    }
}