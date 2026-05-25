using MTG_Emulator.Extensions;
using MTG_Emulator.Unity.Synchronization.Enums;
using MTG_Emulator.Unity.Synchronization.Events;
using UnityEngine;
using UnityEngine.EventSystems;

public class FreeDropZone : MonoBehaviour, IDropHandler
{
    public ZoneType ZoneType => zoneType;
    [SerializeField]
    private ZoneType zoneType;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dragged = eventData.pointerDrag;
        if (dragged == null)
            return;

        if (IsTokenAndInvalidZone(dragged, zoneType))
        {
            Destroy(dragged);
            return;
        }

        Drag dragScript = dragged.GetComponent<Drag>();
        if (dragScript != null)
            dragScript.WasDropped = true;

        var card = dragged.GetComponent<Card>();
        if (card != null)
        {
            card.SetZones(zoneType);
            if (SignalRClient.Instance != null)
                SignalRClient.Instance.Broadcast(new MoveCardEvent(GameSession.PlayerId, card.Identifier)
                {
                    Position = transform.position.ToSystem2(),
                    Zone = card.CurrentZone,
                });
        }

        var token = dragged.GetComponent<Token>();
        if (token != null)
            token.SetZones(zoneType);

        dragged.transform.SetParent(transform, true);
        dragged.transform.SetAsLastSibling();
        dragged.transform.localScale = Vector3.one;
        dragged.transform.localRotation = Quaternion.identity;
    }

    private bool IsTokenAndInvalidZone(GameObject dragged, ZoneType targetZone)
    {
        Token token = dragged.GetComponent<Token>();
        if (token == null)
            return false;
        return targetZone != ZoneType.Bf;
    }
}