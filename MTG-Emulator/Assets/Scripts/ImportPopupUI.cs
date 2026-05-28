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

    private DeckDto createdDeck;
    private List<string> selectedCommanders = new();
    private string originalCardList = "";


    private void Start()
    {
        if (!ensureValidUI())
            Destroy(this);

        importPopup.SetActive(false);
        commanderPickerPanel.SetActive(false);
    }

    private bool ensureValidUI()
    {
        bool valid = true;

        Validate(importPopup);
        Validate(commanderPickerPanel);
        Validate(importFormPanel);

        return valid;

        void Validate(GameObject obj)
        {
            if (obj)
                return;

            Debug.LogError($"{obj.name} was not set, ensure it is not null.");
            valid = false;
        }
    }

    public void OpenImportPopup()
    {
        importPopup.SetActive(true);
        importFormPanel.SetActive(true);
        commanderPickerPanel.SetActive(false);
    }

    public void ClosePopup()
    {
        importPopup.SetActive(false);
    }

    public void SubmitDeck()
    {
        string deckName = deckNameField.text.Trim();
        string importText = importField.text.Trim();

        if (string.IsNullOrEmpty(deckName) || string.IsNullOrEmpty(importText))
        {
            UIPopup.Instance.Show("Deck name or card list is empty.");
            return;
        }

        if (!validateCardList(importText, out string validationError))
        {
            UIPopup.Instance.Show("Invalid card list: " + validationError);
            return;
        }

        originalCardList = importText;

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
                createdDeck = deck;
                selectedCommanders.Clear();
                showCommanderPicker(deck);
            },
            error => UIPopup.Instance.Show("Failed to create deck: " + error)
        ));
    }

    private bool validateCardList(string cardList, out string errorMessage)
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

    private void showCommanderPicker(DeckDto deck)
    {
        Debug.Log(
            $"ShowCommanderPicker called. importPopup={importPopup?.activeSelf}, importFormPanel={importFormPanel?.activeSelf}, commanderPickerPanel={commanderPickerPanel?.activeSelf}");

        if (importFormPanel != null)
            importFormPanel.SetActive(false);
        if (commanderPickerPanel != null)
            commanderPickerPanel.SetActive(true);

        Debug.Log(
            $"After set: commanderPickerPanel={commanderPickerPanel?.activeSelf}, parent={commanderPickerPanel?.transform.parent.gameObject.activeSelf}");
        updateCommandZoneLabel();

        foreach (Transform child in cardPickerListParent)
            Destroy(child.gameObject);

        if (deck.Cards.Count == 0)
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
                addButton.onClick.AddListener(() => addToCommandZone(cardName));
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)cardPickerListParent);

        doneButton.onClick.RemoveAllListeners();
        doneButton.onClick.AddListener(OnDone);
    }

    private void addToCommandZone(string cardName)
    {
        if (selectedCommanders.Contains(cardName))
            return;

        selectedCommanders.Add(cardName);
        updateCommandZoneLabel();

        GameObject entry = Instantiate(commandZoneEntryPrefab, commandZoneParent);
        TMP_Text entryText = entry.GetComponentInChildren<TMP_Text>();
        if (entryText != null)
            entryText.text = cardName;

        Button removeButton = entry.transform.Find("RemoveButton")?.GetComponent<Button>();
        if (removeButton != null)
            removeButton.onClick.AddListener(() => removeFromCommandZone(cardName, entry));

        saveCommandersToBackend();
    }

    private void removeFromCommandZone(string cardName, GameObject entry)
    {
        selectedCommanders.Remove(cardName);
        Destroy(entry);
        updateCommandZoneLabel();
        saveCommandersToBackend();
    }

    private void saveCommandersToBackend()
    {
        if (createdDeck == null)
            return;

        StartCoroutine(APIManager.Instance.UpdateDeckCommander(
            createdDeck.DeckId,
            new List<string>(selectedCommanders),
            createdDeck.DeckName,
            originalCardList,
            result =>
            {
                if (result != null)
                    Debug.Log("Commanders saved: " + string.Join(", ", result.CommandZone.Select(c => c.Name)));
                else
                    Debug.Log("Commanders saved successfully.");
            },
            error => UIPopup.Instance.Show("Failed to save commanders: " + error)
        ));
    }

    private void updateCommandZoneLabel()
    {
        if (commandZoneLabel != null)
            commandZoneLabel.text = $"Command Zone ({selectedCommanders.Count})";
    }

    private void OnDone()
    {
        if (createdDeck == null)
            return;
        StartCoroutine(loadDeckScene(createdDeck));
        ClosePopup();
    }

    private IEnumerator loadDeckScene(DeckDto deck)
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