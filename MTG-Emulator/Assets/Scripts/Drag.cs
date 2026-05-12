using UnityEngine;
using UnityEngine.EventSystems;

public class Drag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform parentToReturnTo;
    private Transform dragParent;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Canvas canvas;

    public bool WasDropped { get; set; }

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        parentToReturnTo = transform.parent;
        dragParent = canvas.transform;

        transform.SetParent(dragParent, true);
        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
        WasDropped = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        if (!WasDropped)
        {
            transform.SetParent(parentToReturnTo, true);
        }
    }
}