using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using MTG_Emulator.Extensions;
using MTG_Emulator.Unity.Synchronization.Enums;
using MTG_Emulator.Unity.Synchronization.Events;

public class Drag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform parentToReturnTo;
    private Transform dragParent;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Canvas canvas;
    private Card card;
    private Tapable tapable;

    private List<(Tapable tapable, Vector2 offset)> dragGroup = new();

    public bool WasDropped { get; set; }

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        card = GetComponent<Card>();
        tapable = GetComponent<Tapable>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentToReturnTo = transform.parent;
        dragParent = canvas.transform;

        transform.SetParent(dragParent, true);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
        WasDropped = false;
        dragGroup.Clear();

        if (SelectionManager.Instance != null && SelectionManager.Instance.HasSelection)
        {
            foreach (Tapable t in SelectionManager.Instance.SelectedCards)
            {
                if (t.gameObject == gameObject)
                    continue;

                Vector2 offset = (Vector2)t.transform.position - (Vector2)transform.position;
                dragGroup.Add((t, offset));

                t.transform.SetParent(dragParent, true);
                t.transform.SetAsLastSibling();

                var cg = t.GetComponent<CanvasGroup>();
                if (cg != null)
                    cg.blocksRaycasts = false;
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
        foreach (var (t, offset) in dragGroup)
            t.transform.position = (Vector2)eventData.position + offset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (!WasDropped)
            transform.SetParent(parentToReturnTo, true);

        Transform dropZone = WasDropped ? transform.parent : null;

        foreach (var (t, _) in dragGroup)
        {
            var cg = t.GetComponent<CanvasGroup>();
            if (cg != null)
                cg.blocksRaycasts = true;

            if (WasDropped && dropZone != null)
            {
                ZonesDrop zonesDrop = dropZone.GetComponent<ZonesDrop>();
                FreeDropZone freeDropZone = dropZone.GetComponent<FreeDropZone>();

                ZoneType? targetZone = null;
                if (zonesDrop != null)
                    targetZone = zonesDrop.ZoneType;
                else if (freeDropZone != null)
                    targetZone = freeDropZone.ZoneType;

                if (targetZone.HasValue)
                {
                    Token token = t.GetComponent<Token>();
                    if (token != null && targetZone.Value != ZoneType.Bf)
                    {
                        Destroy(t.gameObject);
                        continue;
                    }

                    t.transform.SetParent(dropZone, true);

                    // Apply same visual logic as ZonesDrop
                    if (zonesDrop != null && zonesDrop.DownScale)
                    {
                        t.transform.localScale = zonesDrop.DroppedScale;
                        t.transform.localRotation = Quaternion.Euler(zonesDrop.DroppedRotation);
                    }
                    else
                    {
                        t.transform.localScale = Vector3.one;
                        t.transform.localRotation = Quaternion.identity;
                    }

                    Card cardComp = t.GetComponent<Card>();
                    if (cardComp != null)
                    {
                        cardComp.SetZones(targetZone.Value);
                        if (SignalRClient.Instance != null)
                            SignalRClient.Instance.Broadcast(new MoveCardEvent(
                                GameSession.PlayerId, cardComp.Identifier)
                            {
                                Position = dropZone.position.ToSystem2(),
                                Zone = cardComp.CurrentZone,
                            });
                    }

                    token = t.GetComponent<Token>();
                    if (token != null)
                        token.SetZones(targetZone.Value);
                }
            }
            else
            {
                t.transform.SetParent(parentToReturnTo, true);
            }
        }

        dragGroup.Clear();
    }
}