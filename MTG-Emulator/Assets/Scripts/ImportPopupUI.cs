using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MTG_Emulator.Unity.Db.DTO.DeckDTO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ImportPopupUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField]
    private GameObject importPopup;

    [SerializeField]
    private GameObject importFormPanel;

    [SerializeField]
    private GameObject commanderPickerPanel;

    [Header("Import Form")]
    public TMP_InputField deckNameField;

    public TMP_InputField importField;

    [Header("Commander Picker")]
    [SerializeField]
    private Transform cardPickerListParent;

    [SerializeField]
    private GameObject cardPickerRowPrefab;

    [SerializeField]
    private Transform commandZoneParent;

    [SerializeField]
    private GameObject commandZoneEntryPrefab;

    [SerializeField]
    private TMP_Text commandZoneLabel;

    [SerializeField]
    private Button doneButton;

    private DeckDto _createdDeck;
    private List<string> _selectedCommanders = new();
    private string _originalCardList = "";


    private void Start()
    {
        if (importPopup != null)
            importPopup.SetActive(false);
        if (commanderPickerPanel != null)
            commanderPickerPanel.SetActive(false);
    }

    public void OpenImportPopup()
    {
        if (importPopup != null)
            importPopup.SetActive(true);
        if (importFormPanel != null)
            importFormPanel.SetActive(true);
        if (commanderPickerPanel != null)
            commanderPickerPanel.SetActive(false);
    }

    public void ClosePopup()
    {
        if (importPopup != null)
            importPopup.SetActive(false);
    }

    public void SubmitDeck()
    {
        string deckName = deckNameField.text.Trim();
        string importText = importField.text
            .Replace("\r\n", "\n")
            .Replace("\r", "\n")
            .Trim();

        if (string.IsNullOrEmpty(deckName) || string.IsNullOrEmpty(importText))
        {
            Debug.LogError("Deck name or card list is empty");
            return;
        }

        if (!ValidateCardList(importText, out string validationError))
        {
            Debug.LogError("Invalid card list format: " + validationError);
            return;
        }

        _originalCardList = importText;

        CreateDeckDto dto = new CreateDeckDto
        {
            DeckName = deckName,
            CardList = importText,
            CommandZone = new List<string>()
        };

        StartCoroutine(APIManager.Instance.CreateDeck(
            dto,
            deck =>
            {
                Debug.Log($"Deck created: ID={deck.DeckId}, Cards={deck.Cards?.Count ?? -1}");
                _createdDeck = deck;
                _selectedCommanders.Clear();
                ShowCommanderPicker(deck);
            },
            error => Debug.LogError("API ERROR: " + error)
        ));
    }

    private bool ValidateCardList(string cardList, out string errorMessage)
    {
        errorMessage = "";
        string[] lines = cardList.Split('\n');

        foreach (string line in lines)
        {
            string trimmed = line.Trim();
            if (string.IsNullOrEmpty(trimmed))
                continue;

            string[] parts = trimmed.Split(' ');

            if (parts.Length < 2 || !int.TryParse(parts[0], out int quantity) || quantity <= 0)
            {
                errorMessage = $"Invalid line: \"{trimmed}\"\nExpected format: 1 Card Name";
                return false;
            }
        }

        return true;
    }

    private void ShowCommanderPicker(DeckDto deck)
    {
        Debug.Log(
            $"ShowCommanderPicker called. importPopup={importPopup?.activeSelf}, importFormPanel={importFormPanel?.activeSelf}, commanderPickerPanel={commanderPickerPanel?.activeSelf}");

        if (importFormPanel != null)
            importFormPanel.SetActive(false);
        if (commanderPickerPanel != null)
            commanderPickerPanel.SetActive(true);

        Debug.Log(
            $"After set: commanderPickerPanel={commanderPickerPanel?.activeSelf}, parent={commanderPickerPanel?.transform.parent.gameObject.activeSelf}");
        UpdateCommandZoneLabel();

        foreach (Transform child in cardPickerListParent)
            Destroy(child.gameObject);

        if (deck.Cards == null || deck.Cards.Count == 0)
        {
            Debug.LogWarning("No cards found in deck.");
            return;
        }

        var groups = deck.Cards.GroupBy(c => c.Name).OrderBy(g => g.Key).ToList();
        Debug.Log($"Card groups count: {groups.Count}. cardPickerListParent={cardPickerListParent?.name}");

        foreach (var group in groups)
        {
            GameObject row = Instantiate(cardPickerRowPrefab, cardPickerListParent);

            TMP_Text nameText = row.GetComponentInChildren<TMP_Text>();
            if (nameText != null)
                nameText.text = $"{group.Count()}x {group.Key}";

            Button addButton = row.GetComponentInChildren<Button>();
            if (addButton != null)
            {
                string cardName = group.Key;
                addButton.onClick.AddListener(() => AddToCommandZone(cardName));
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)cardPickerListParent);

        doneButton.onClick.RemoveAllListeners();
        doneButton.onClick.AddListener(OnDone);
    }

    private void AddToCommandZone(string cardName)
    {
        if (_selectedCommanders.Contains(cardName))
            return;

        _selectedCommanders.Add(cardName);
        UpdateCommandZoneLabel();

        GameObject entry = Instantiate(commandZoneEntryPrefab, commandZoneParent);
        TMP_Text entryText = entry.GetComponentInChildren<TMP_Text>();
        if (entryText != null)
            entryText.text = cardName;

        Button removeButton = entry.transform.Find("RemoveButton")?.GetComponent<Button>();
        if (removeButton != null)
            removeButton.onClick.AddListener(() => RemoveFromCommandZone(cardName, entry));

        SaveCommandersToBackend();
    }

    private void RemoveFromCommandZone(string cardName, GameObject entry)
    {
        _selectedCommanders.Remove(cardName);
        Destroy(entry);
        UpdateCommandZoneLabel();
        SaveCommandersToBackend();
    }

    private void SaveCommandersToBackend()
    {
        if (_createdDeck == null)
            return;

        StartCoroutine(APIManager.Instance.UpdateDeckCommander(
            _createdDeck.DeckId,
            new List<string>(_selectedCommanders),
            _createdDeck.DeckName,
            _originalCardList,
            result =>
            {
                if (result != null)
                    Debug.Log("Commanders saved: " + string.Join(", ", result.CommandZone.Select(c => c.Name)));
                else
                    Debug.Log("Commanders saved successfully.");
            },
            error => Debug.LogError("Failed to save commanders: " + error)
        ));
    }

    private void UpdateCommandZoneLabel()
    {
        if (commandZoneLabel != null)
            commandZoneLabel.text = $"Command Zone ({_selectedCommanders.Count})";
    }

    private void OnDone()
    {
        if (_createdDeck == null)
            return;
        StartCoroutine(LoadDeckScene(_createdDeck));
        ClosePopup();
    }

    private IEnumerator LoadDeckScene(DeckDto deck)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync("Deck_Viewer");
        while (!op.isDone)
            yield return null;

        yield return null;

        if (DeckViewer.Instance != null)
            DeckViewer.Instance.ShowDeck(deck);
        else
            Debug.LogError("DeckViewer.Instance not found after scene load!");
    }
}