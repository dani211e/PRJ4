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
    
            if (signalRClient == null)
            {
                Debug.LogWarning("SignalRClient not found in scene.");
                return;
            }
    
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
                var obj = Instantiate(cardPrefab, zones.GetTransformFor(ZoneType.Hand));

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
                Debug.Log($"move event rec {e.Identifier}");
                if (!e.Position.HasValue)
                    return;

                var c = CardManager.Get(e.PlayerIndex, e.Identifier);
                var zone = zones.GetTransformFor(e.Zone);
                Assert.IsNotNull(c);
                Assert.IsNotNull(zone);
                c.transform.SetParent(zone, false);

                //var newPos = e.Position.Value.ToUnity3();
                //c.transform.position = new Vector3(newPos.x, newPos.y - 100, 0);
            });
        }
    }
}