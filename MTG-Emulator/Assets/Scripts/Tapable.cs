using MTG_Emulator.Unity.Synchronization.Enums;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tapable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private bool isTapped = false;
    private bool isHovered = false;
    private ZoneType currentZone;

    public void SetZone(ZoneType zone)
    {
        currentZone = zone;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (currentZone != ZoneType.Bf || !isHovered)
                return;

            transform.Rotate(0, 0, isTapped ? 90.0f : -90.0f);
            isTapped = !isTapped;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
    }
}