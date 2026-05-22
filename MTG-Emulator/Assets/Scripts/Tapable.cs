using MTG_Emulator.Unity.Synchronization.Enums;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tapable : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private bool isTapped = false;
    private bool isHovered = false;
    private bool isSelected = false;
    private ZoneType currentZone;
    private Outline selectionOutline;

    private void Awake()
    {
        selectionOutline = GetComponent<Outline>();
        if (selectionOutline == null)
            selectionOutline = gameObject.AddComponent<Outline>();

        selectionOutline.enabled = false;
        selectionOutline.effectColor = Color.yellow;
        selectionOutline.effectDistance = new Vector2(3, 3);
    }

    public void SetZone(ZoneType zone)
    {
        currentZone = zone;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (currentZone != ZoneType.Bf)
                return;

            if (SelectionManager.Instance.HasSelection)
                return;

            if (!isHovered)
                return;

            TapToggle();
        }
    }

    public void TapToggle()
    {
        transform.Rotate(0, 0, isTapped ? 90.0f : -90.0f);
        isTapped = !isTapped;
    }

    public void HighLight()
    {
        selectionOutline.enabled = true;
        selectionOutline.effectColor = Color.cyan;
    }

    public void UnHighlight()
    {
        if (!isSelected)
            selectionOutline.enabled = false;
        else
            selectionOutline.effectColor = Color.yellow;
    }

    public void Select()
    {
        isSelected = true;
        selectionOutline.enabled = true;
        selectionOutline.effectColor = Color.yellow;
        SelectionManager.Instance.Register(this);
    }

    public void Deselect()
    {
        isSelected = false;
        selectionOutline.enabled = false;
        SelectionManager.Instance.Unregister(this);
    }

    public void OnPointerEnter(PointerEventData eventData) => isHovered = true;
    public void OnPointerExit(PointerEventData eventData) => isHovered = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (isSelected)
                Deselect();
            else
                Select();
        }
    }
}