using System;
using MTG_Emulator.Extensions;
using MTG_Emulator.Unity.Synchronization.Enums;
using MTG_Emulator.Unity.Synchronization.Events;
using UnityEngine;
using UnityEngine.EventSystems;

public class FreeDropZone : MonoBehaviour, IDropHandler
{
    [SerializeField]
    private ZoneType zoneType;

    private float left;
    private float right;
    private float top;
    private float bottom;

    private void Awake()
    {
        var zoneRect = ((RectTransform)transform).rect;
        left = zoneRect.xMin;
        right = zoneRect.xMax;
        top = zoneRect.yMin;
        bottom = zoneRect.yMax;
    }

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

        // Make sure card stays within bounds of zone
        // TODO: Make card size an easily accessible property, this is annoying to do.
        var cardSize = ((RectTransform)dragged.transform).sizeDelta;
        var cardW = cardSize.x / 2;
        var cardH = cardSize.y / 2;

        var clampedX = Math.Clamp(dragged.transform.localPosition.x, left + cardW, right - cardW);
        var clampedY = Math.Clamp(dragged.transform.localPosition.y, top + cardH, bottom - cardH);

        dragged.transform.localPosition = new Vector3(clampedX, clampedY, 0);

        var card = dragged.GetComponent<Card>();
        if (card != null)
        {
            card.SetZones(zoneType);

            var relPos = dragged.transform.localPosition;
            SignalRClient.Instance.Broadcast(new MoveCardEvent(GameSession.PlayerId, card.Identifier)
            {
                Position = relPos.ToSystem2(),
                Zone = card.CurrentZone,
            });
        }
    }
}