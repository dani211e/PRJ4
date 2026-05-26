using MTG_Emulator.Extensions;
using MTG_Emulator.Unity.Synchronization.Enums;
using MTG_Emulator.Unity.Synchronization.Events;
using UnityEngine;
using UnityEngine.EventSystems;

public class FreeDropZone : MonoBehaviour, IDropHandler
{
    [SerializeField]
    private ZoneType zoneType;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dragged = eventData.pointerDrag;
        if (dragged == null)
            return;

        Drag dragScript = dragged.GetComponent<Drag>();
        if (dragScript != null)
        {
            dragScript.WasDropped = true;
        }

        // Re-setting the parent needs to happen before we send the event,
        // as we rely on the relative parent position (localpos).
        dragged.transform.SetParent(transform, true);
        dragged.transform.SetAsLastSibling();
        dragged.transform.localScale = Vector3.one;
        dragged.transform.localRotation = Quaternion.identity;

        var card = dragged.GetComponent<Card>();
        if (card != null)
        {
            card.SetZones(zoneType);
            SignalRClient.Instance.Broadcast(new MoveCardEvent(GameSession.PlayerId, card.Identifier)
            {
                Position = transform.position.ToSystem2(),
                Zone = card.CurrentZone,
            });
        }
    }
}