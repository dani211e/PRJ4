using MTG_Emulator.Extensions;
using MTG_Emulator.Unity.Synchronization.Enums;
using MTG_Emulator.Unity.Synchronization.Events;
using UnityEngine;
using UnityEngine.EventSystems;

public class ZonesDrop : MonoBehaviour, IDropHandler
{
    [SerializeField]
    private Vector3 droppedScale = new(0.8f, 0.8f, 1f);

    [SerializeField]
    private Vector3 droppedRotation = new(0f, 0f, -90f);

    [SerializeField]
    private ZoneType zoneType;

    private bool shouldDownScale => zoneType is ZoneType.Exile or ZoneType.Graveyard;

    public void OnDrop(PointerEventData eventData)
    {
        var dragged = eventData.pointerDrag;
        if (dragged == null)
            return;

        Drag dragScript = dragged.GetComponent<Drag>();
        if (dragScript != null)
            dragScript.WasDropped = true;

        dragged.transform.SetParent(transform, false);

        var card = dragged.GetComponent<Card>();
        if (card)
        {
            card.SetZones(zoneType);
            SignalRClient.Instance.Broadcast(new MoveCardEvent(GameSession.PlayerId, card.Identifier)
            {
                Zone = zoneType,
            });
        }
        
        dragged.transform.localScale = shouldDownScale ? droppedScale : Vector3.one;
        dragged.transform.localRotation = shouldDownScale ? Quaternion.Euler(droppedRotation) : Quaternion.identity;
    }
}