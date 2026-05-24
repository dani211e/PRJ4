using MTG_Emulator.Cards.Extensions;
using MTG_Emulator.Extensions;
using MTG_Emulator.Threading;
using MTG_Emulator.Unity.Synchronization.Enums;
using MTG_Emulator.Unity.Synchronization.Events;
using MTG_Emulator.Zones;
using UnityEngine;
using UnityEngine.Assertions;

namespace MTG_Emulator.Cards
{
    public class CardListener : MonoBehaviour
    {
        private SignalRClient signalRClient;
        private CardManager cardManager;

        [SerializeField]
        private GameObject cardPrefab;

        [SerializeField]
        private Transform battlefield;

        private ZoneMapping zones;

        private void Awake()
        {
            signalRClient = FindFirstObjectByType<SignalRClient>();
            signalRClient.OnNewCardEvent += spawnNewCard;
            signalRClient.OnMoveCardEvent += moveCard;

            zones = GetComponent<ZoneMapping>();
            if (!zones)
                Debug.LogError("Failed to get zone mappings");
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

                var zone = zones.GetTransformFor(ZoneType.Hand);
                var obj = Instantiate(cardPrefab, zone);
                obj.RemoveComponent<Drag>();

                var cardInfo = e.Card.ToCardInfo();
                cardInfo.Identifier = e.Identifier;
                var c = obj.GetComponent<Card>();

                c.Setup(cardInfo);
                CardManager.AddObject(e.PlayerIndex, cardInfo, obj);
            });
        }

        private void moveCard(object _, MoveCardEvent e)
        {
            MainThreadDispatcher.Enqueue(() =>
            {
                var c = CardManager.Get(e.PlayerIndex, e.Identifier);

                var zone = zones.GetTransformFor(e.Zone);
                c.transform.SetParent(zone, false);

                if (!e.Position.HasValue)
                    return;

                var newPos = e.Position.Value.ToUnity2();
                //We need to mirror the y coord to match the flipped enemy side
                c.transform.localPosition = new Vector3(newPos.x, -newPos.y, 0);
            });
        }
    }
}