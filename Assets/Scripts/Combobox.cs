using System.Linq;
using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

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

        _persistentOptions = options.ToList();
        _persistentValue = 0;

        _isFocused = false;
        _maintainSearchEditOnCurrentFrame = false;

        onValueChanged.AddListener(Handle_OnDropdownValueChanged);

        _searchField.onValueChanged.AddListener(Handle_OnSearchChanged);
        _searchField.onEndEdit.AddListener(Handle_OnSearchEnded);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        onValueChanged.RemoveListener(Handle_OnDropdownValueChanged);

        _searchField.onValueChanged.RemoveListener(Handle_OnSearchChanged);
        _searchField.onEndEdit.RemoveListener(Handle_OnSearchEnded);
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
        }
        else
        {
            options = _persistentOptions.ToList();

            RefreshDropdown();
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

    private Toggle[] GetToggles(bool includeInactive)
    {
        return GetComponentsInChildren<Toggle>(includeInactive);
    }

    private string TextToTerm(string text)
    {
        return text.ToLower().Trim();
    }

    private void Handle_OnDropdownValueChanged(int newValue)
    {

    }

    private void Handle_OnSearchChanged(string newValue)
    {
        if (!_isFocused)
        {
            _persistentOptions = options.ToList();
            _isFocused = true;
        }

        RefreshOptions();

        SelectSearchField();
        _maintainSearchEditOnCurrentFrame = true;
    }

    private void Handle_OnSearchEnded(string finalValue)
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
}