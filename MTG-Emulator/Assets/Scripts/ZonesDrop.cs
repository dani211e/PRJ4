using MTG_Emulator.Extensions;
using MTG_Emulator.Unity.Synchronization.Enums;
using MTG_Emulator.Unity.Synchronization.Events;
using UnityEngine;
using UnityEngine.EventSystems;

public class ZonesDrop : MonoBehaviour, IDropHandler
{
    [SerializeField] private bool downScale = false;
    [SerializeField] private Vector3 droppedScale = new(0.8f, 0.8f, 1f);
    [SerializeField] private Vector3 droppedRotation = new(0f, 0f, -90f);
    [SerializeField] private ZoneType zoneType;

    public bool DownScale => downScale;
    public Vector3 DroppedScale => droppedScale;
    public Vector3 DroppedRotation => droppedRotation;
    public ZoneType ZoneType => zoneType;

    public void OnDrop(PointerEventData eventData)
    {
        var dragged = eventData.pointerDrag;
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

        dragged.transform.SetParent(transform, false);

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

    private bool IsTokenAndInvalidZone(GameObject dragged, ZoneType targetZone)
    {
        Token token = dragged.GetComponent<Token>();
        if (token == null)
            return false;
        return targetZone != ZoneType.Bf;
    }
}