using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectionManager : MonoBehaviour
{
    public static SelectionManager Instance;

    [SerializeField] private RectTransform selectionBox;
    [SerializeField] private Canvas canvas;

    private Vector2 startPos;
    private bool isDragging = false;
    private List<Tapable> selectedCards = new();

    private void Awake()
    {
        Instance = this;
        selectionBox.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && HasSelection)
        {
            foreach (Tapable t in new List<Tapable>(selectedCards))
                t.TapToggle(broadcast: true);
        }

        if (Input.GetMouseButtonDown(0))
        {
            // Check if we clicked directly on a Tapable
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            bool hitCard = results.Exists(r => r.gameObject.GetComponent<Tapable>() != null 
                                               || r.gameObject.GetComponentInParent<Tapable>() != null);

            if (!hitCard)
            {
                if (!Input.GetKey(KeyCode.LeftShift))
                    DeselectAll();

                startPos = Input.mousePosition;
                isDragging = true;
                selectionBox.gameObject.SetActive(true);
            }
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            UpdateSelectionBox(Input.mousePosition);
            // Highlight cards inside box while dragging
            HighlightCardsInBox();
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;
            selectionBox.gameObject.SetActive(false);

            float dragSize = Vector2.Distance(startPos, Input.mousePosition);
            if (dragSize < 5f)
            { 
                return;
            }

            // Finalize selection
            SelectCardsInBox();
        }
    }

    private void UpdateSelectionBox(Vector2 currentPos)
    {
        Vector2 size = new Vector2(
            Mathf.Abs(currentPos.x - startPos.x),
            Mathf.Abs(currentPos.y - startPos.y)
        );

        Vector2 origin = new Vector2(
            Mathf.Min(startPos.x, currentPos.x),
            Mathf.Min(startPos.y, currentPos.y)
        );

        selectionBox.position = new Vector3(origin.x + size.x / 2, origin.y + size.y / 2, 0);
        selectionBox.sizeDelta = size / canvas.scaleFactor;
    }

    private void HighlightCardsInBox()
    {
        Rect selectionRect = new Rect(
            Mathf.Min(startPos.x, Input.mousePosition.x),
            Mathf.Min(startPos.y, Input.mousePosition.y),
            Mathf.Abs(Input.mousePosition.x - startPos.x),
            Mathf.Abs(Input.mousePosition.y - startPos.y)
        );

        foreach (Tapable tapable in FindObjectsOfType<Tapable>())
        {
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, tapable.transform.position);
            if (selectionRect.Contains(screenPos))
                tapable.HighLight();
            else
                tapable.UnHighlight();
        }
    }

    private void SelectCardsInBox()
    {
        Rect selectionRect = new Rect(
            Mathf.Min(startPos.x, Input.mousePosition.x),
            Mathf.Min(startPos.y, Input.mousePosition.y),
            Mathf.Abs(Input.mousePosition.x - startPos.x),
            Mathf.Abs(Input.mousePosition.y - startPos.y)
        );

        foreach (Tapable tapable in FindObjectsOfType<Tapable>())
        {
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, tapable.transform.position);
            if (selectionRect.Contains(screenPos))
                tapable.Select();
            else if (!Input.GetKey(KeyCode.LeftShift))
                tapable.Deselect();
        }
    }

    public void DeselectAll()
    {
        foreach (Tapable t in new List<Tapable>(selectedCards))
            t.Deselect();
        selectedCards.Clear();
    }

    public void Register(Tapable tapable)
    {
        if (!selectedCards.Contains(tapable))
            selectedCards.Add(tapable);
    }

    public void Unregister(Tapable tapable)
    {
        selectedCards.Remove(tapable);
    }

    public bool HasSelection => selectedCards.Count > 0;
    public List<Tapable> SelectedCards => selectedCards;
}