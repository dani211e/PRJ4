using System;
using MTG_Emulator.Threading;
using MTG_Emulator.Unity.Synchronization.Enums;
using MTG_Emulator.Unity.Synchronization.Events;
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
    private Guid identifier;

    private void Awake()
    {
        var graphic = GetComponent<Graphic>() ?? GetComponentInChildren<Graphic>();
    
        if (graphic != null)
        {
            selectionOutline = graphic.gameObject.GetComponent<Outline>();
            if (selectionOutline == null)
                selectionOutline = graphic.gameObject.AddComponent<Outline>();
        }
        else
        {
            selectionOutline = GetComponent<Outline>();
            if (selectionOutline == null)
                selectionOutline = gameObject.AddComponent<Outline>();
        }

        selectionOutline.enabled = false;
        selectionOutline.effectColor = Color.yellow;
        selectionOutline.effectDistance = new Vector2(3, 3);
    }

    private void OnEnable()
    {
        if (SignalRClient.Instance != null)
            SignalRClient.Instance.OnTapCardEvent += OnTapCardReceived;
    }

    private void OnDisable()
    {
        if (SignalRClient.Instance != null)
            SignalRClient.Instance.OnTapCardEvent -= OnTapCardReceived;
    }

    public void SetZone(ZoneType zone)
    {
        currentZone = zone;
    }

    public void SetIdentifier(Guid id)
    {
        identifier = id;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"Space pressed. Zone: {currentZone}, SelectionManager null: {SelectionManager.Instance == null}, HasSelection: {SelectionManager.Instance?.HasSelection}, isHovered: {isHovered}");
        
            if (currentZone != ZoneType.Bf)
                return;

            if (SelectionManager.Instance != null && SelectionManager.Instance.HasSelection)
                return;

            if (!isHovered)
                return;

            TapToggle(broadcast: true);
        }
    }

    public void TapToggle(bool broadcast = false)
    {
        transform.Rotate(0, 0, isTapped ? 90.0f : -90.0f);
        isTapped = !isTapped;

        if (broadcast && SignalRClient.Instance != null)
            SignalRClient.Instance.Broadcast(
                new TapCardEvent(GameSession.PlayerId, identifier, isTapped)
            );
    }

    private void OnTapCardReceived(object sender, TapCardEvent e)
    {
        if (e.Identifier != identifier)
            return;

        MainThreadDispatcher.Enqueue(() =>
        {
            if (isTapped != e.IsTapped)
                TapToggle(broadcast: false);
        });
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
        SelectionManager.Instance?.Register(this);
    }

    public void Deselect()
    {
        isSelected = false;
        selectionOutline.enabled = false;
        SelectionManager.Instance?.Unregister(this);
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