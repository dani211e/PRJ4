using UnityEngine;
using UnityEngine.EventSystems;

public class FreeDropZone : MonoBehaviour, IDropHandler
{

    [SerializeField] private CardZonesTypes zoneType;
    public void OnDrop(PointerEventData eventData)
    {
        GameObject dragged = eventData.pointerDrag;
        if (dragged == null)
            return;

        Drag dragScript = dragged.GetComponent<Drag>();
        var card =  dragged.GetComponent<Card>();
        if (dragScript != null)
        {
            dragScript.WasDropped = true;
        }

        if (card != null)
        {
            card.SetZones(zoneType);
        }


        dragged.transform.SetParent(transform, true);
        dragged.transform.SetAsLastSibling();
    }
}