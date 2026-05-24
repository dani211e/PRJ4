using MTG_Emulator.Extensions;
using MTG_Emulator.Unity.Synchronization.Enums;
using MTG_Emulator.Unity.Synchronization.Events;
using UnityEngine;
using UnityEngine.EventSystems;

public class ZonesDrop : MonoBehaviour, IDropHandler
{
    [SerializeField]
    private bool downScale = false;

    [SerializeField]
    private Vector3 droppedScale = new(0.8f, 0.8f, 1f);

    [SerializeField]
    private Vector3 droppedRotation = new(0f, 0f, -90f);

    [SerializeField]
    private ZoneType zoneType;

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
                Position = transform.position.ToSystem2(),
                Zone = card.CurrentZone,
            });
        }


        if (downScale)
        {
            dragged.transform.localScale = droppedScale;
            dragged.transform.localRotation = Quaternion.Euler(droppedRotation);
        }
        else
        {
            dragged.transform.localScale = Vector3.one;
            dragged.transform.localRotation = Quaternion.identity;
        }
    }
}