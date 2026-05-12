using UnityEngine;
using UnityEngine.EventSystems;

public class FreeDropZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        GameObject dragged = eventData.pointerDrag;
        if (dragged == null)
            return;

        Drag dragScript = dragged.GetComponent<Drag>();
        if (dragScript != null)
            dragScript.WasDropped = true;

        dragged.transform.SetParent(transform, true);
        dragged.transform.SetAsLastSibling();
    }
}