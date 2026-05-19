using UnityEngine;
using UnityEngine.EventSystems;

public class ZonesDrop : MonoBehaviour, IDropHandler
{
    [SerializeField] private bool downScale = false;
    [SerializeField] private Vector3 droppedScale = new(0.8f, 0.8f, 1f);
    [SerializeField] private Vector3 droppedRotation = new(0f, 0f, -90f);

    public void OnDrop(PointerEventData eventData)
    {
        var dragged = eventData.pointerDrag;
        if (dragged == null)
            return;

        Drag dragScript = dragged.GetComponent<Drag>();
        if (dragScript != null)
            dragScript.WasDropped = true;

        dragged.transform.SetParent(transform, false);

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