using System.Linq;
using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Combobox : TMP_Dropdown
{
    [SerializeField]
    private TMP_InputField _searchField;

    private List<OptionData> _persistentOptions;
    private int _persistentValue;

    private bool _isFocused;
    private bool _maintainSearchEditOnCurrentFrame;

    protected override void Awake()
    {
        base.Awake();

        _searchField = GetComponentInChildren<TMP_InputField>();

        _persistentOptions = new List<OptionData>();
        _persistentValue = 0;

        _isFocused = false;
        _maintainSearchEditOnCurrentFrame = false;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        _isFocused = false;
        _maintainSearchEditOnCurrentFrame = false;

        _searchField.onValueChanged.AddListener(Handle_OnSearchChanged);
        _searchField.onEndEdit.AddListener(Handle_OnSearchEnded);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        _searchField.onValueChanged.RemoveListener(Handle_OnSearchChanged);
        _searchField.onEndEdit.RemoveListener(Handle_OnSearchEnded);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        Focus();
    }

    public override void OnSubmit(BaseEventData eventData)
    {
        base.OnSubmit(eventData);
        Focus();
    }

    public override void OnCancel(BaseEventData eventData)
    {
        base.OnCancel(eventData);
        UnFocus();
    }

    private void Focus()
    {
        if (!_isFocused)
        {
            _persistentOptions = options.ToList();
            _isFocused = true;
        }

        RefreshOptions();
        ApplyPersistentValue();

        SelectSearchField();
        _maintainSearchEditOnCurrentFrame = true;
    }

    private void UnFocus()
    {
        if (_maintainSearchEditOnCurrentFrame)
        {
            _maintainSearchEditOnCurrentFrame = false;
            return;
        }

        Hide();

        options = _persistentOptions.ToList();
        _isFocused = false;
    }

    private void RefreshOptions()
    {
        string searchTerm = TextToTerm(_searchField.text);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            options = _persistentOptions
                .Where(persistentOption =>
                {
                    string persistenTerm = TextToTerm(persistentOption.text);
                    return persistenTerm.Contains(searchTerm);
                })
                .ToList();

            RefreshDropdown();
            ApplyPersistentValue();
            
            DropdownItem[] items = GetItems(false);

            for (int i = 0; i < items.Length; i++)
            {
                DropdownItem item = items[i];
                int itemIndex = i;
                
                item.toggle.onValueChanged.RemoveAllListeners();
                item.toggle.onValueChanged.AddListener((isOn) => Handle_OnSelectDropdownItem(item.text.text));
            }
        }
        else
        {
            options = _persistentOptions.ToList();

            RefreshDropdown();
            ApplyPersistentValue();
        }
    }

    private void RefreshDropdown()
    {
        float backupAphaFadeSpeed = alphaFadeSpeed;

        alphaFadeSpeed = float.PositiveInfinity;

        Hide();

        alphaFadeSpeed = 0;

        Show();

        alphaFadeSpeed = backupAphaFadeSpeed;
    }

    private void SelectSearchField()
    {
        int backupSelectionAnchorPosition = _searchField.selectionAnchorPosition;
        int backupSelectionStringAnchorPosition = _searchField.selectionStringAnchorPosition;

        _searchField.Select();

        _searchField.selectionAnchorPosition = backupSelectionAnchorPosition;
        _searchField.selectionStringAnchorPosition = backupSelectionStringAnchorPosition;
    }

    private void StorePersistentValue(string selectedText)
    {
        string selectedTerm = TextToTerm(selectedText);

        // Find index of selected value among persistent options
        _persistentValue = _persistentOptions
            .Select((persistentOption, index) => new
            {
                Option = persistentOption,
                Index = index
            })
            .Where(optionToIndex =>
            {
                string optionTerm = TextToTerm(optionToIndex.Option.text);
                return optionTerm.Equals(selectedTerm);
            })
            .Select(optionToIndex => optionToIndex.Index)
            .DefaultIfEmpty(0)
            .FirstOrDefault();
    }

    private void ApplyPersistentValue()
    {
        string persistentTerm = TextToTerm(_persistentOptions[_persistentValue].text);

        // Find index of persistent value among filtered options
        int newValue = options
            .Select((option, index) => new
            {
                Option = option,
                Index = index
            })
            .Where(optionToIndex =>
            {
                string optionTerm = TextToTerm(optionToIndex.Option.text);
                return optionTerm.Equals(persistentTerm);
            })
            .Select(optionToIndex => optionToIndex.Index)
            .DefaultIfEmpty(-1)
            .FirstOrDefault();

        SetValueWithoutNotify(newValue);

        Toggle[] toggles = GetToggles(false);

        for (int i = 0; i < toggles.Length; i++)
        {
            Toggle toggle = toggles[i];
            TMP_Text toggleText = toggle.GetComponentInChildren<TMP_Text>();

            string toggleTerm = TextToTerm(toggleText.text);
            toggle.SetIsOnWithoutNotify(toggleTerm.Equals(persistentTerm));
        }
    }

    private Toggle[] GetToggles(bool includeInactive)
    {
        return GetComponentsInChildren<Toggle>(includeInactive);
    }
    
    private DropdownItem[] GetItems(bool includeInactive)
    {
        return GetComponentsInChildren<DropdownItem>(includeInactive);
    }

    private string TextToTerm(string text)
    {
        return text.ToLower().Trim();
    }

    private void Handle_OnSelectDropdownItem(string selectedText)
    {
        StorePersistentValue(selectedText);
        RefreshDropdown();
        ApplyPersistentValue();
        Hide();
    }

    private void Handle_OnSearchChanged(string newValue)
    {
        Focus();
    }

    private void Handle_OnSearchEnded(string finalValue)
    {
        UnFocus();
    }
}