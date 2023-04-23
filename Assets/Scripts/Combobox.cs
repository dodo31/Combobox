using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class Combobox : TMP_Dropdown
{
    [SerializeField]
    private TMP_InputField _searchField;

    private bool _maintainSearchEditOnCurrentFrame;

    protected override void Awake()
    {
        base.Awake();

        _searchField = GetComponentInChildren<TMP_InputField>();

        _maintainSearchEditOnCurrentFrame = false;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        _searchField.onValueChanged.AddListener(Handle_OnSearchChanged);
        _searchField.onEndEdit.AddListener(Handle_OnSearchEnded);
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        _searchField.onValueChanged.RemoveListener(Handle_OnSearchChanged);
        _searchField.onEndEdit.RemoveListener(Handle_OnSearchEnded);
    }

    public new void Show()
    {
        base.Show();
    }

    public new void Hide()
    {
        base.Hide();
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        foreach (DropdownItem item in GetItems(true))
        {
            item.gameObject.SetActive(true);
        }

        base.OnPointerClick(eventData);
        Focus();
    }

    public override void OnSubmit(BaseEventData eventData)
    {
        base.OnSubmit(eventData);
        UnFocus();
    }

    public override void OnCancel(BaseEventData eventData)
    {
        base.OnCancel(eventData);
        UnFocus();
    }

    private void Focus()
    {
        Canvas listCanvas = GetComponentInChildren<Canvas>(false);

        if (listCanvas == null)
        {
            RefreshDropdownVisibility();
        }

        RefreshOptions();

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
    }

    private void RefreshOptions()
    {
        string searchTerm = TextToTerm(_searchField.text);
        DropdownItem[] items = GetItems(true);

        if (!string.IsNullOrEmpty(searchTerm))
        {
            for (int i = 0; i < items.Length; i++)
            {
                DropdownItem item = items[i];

                bool isTemplateItem = (i == 0);
                bool isReferenceItem = (i == 1);

                if (isTemplateItem || isReferenceItem)
                {
                    item.gameObject.SetActive(true);
                }
                else
                {
                    string itemTerm = TextToTerm(item.text.text);
                    item.gameObject.SetActive(itemTerm.Contains(searchTerm));
                }
            }

            RefreshDropdownList();
        }
        else
        {
            foreach (DropdownItem item in items)
            {
                item.gameObject.SetActive(true);
            }

            RefreshDropdownList();
        }
    }

    private void RefreshDropdownVisibility()
    {
        float backupAphaFadeSpeed = alphaFadeSpeed;
        {
            alphaFadeSpeed = float.PositiveInfinity;
            Hide();

            alphaFadeSpeed = 0;
            Show();
        }
        alphaFadeSpeed = backupAphaFadeSpeed;
    }

    private void RefreshDropdownList()
    {
        DropdownItem[] items = GetItems(false);
        RectTransform templateItem = (RectTransform)items.First().transform;
        RectTransform listTransform = (RectTransform)templateItem.parent;

        float itemsHeight = templateItem.sizeDelta.y;

        templateItem.anchoredPosition = new Vector2
        {
            x = templateItem.anchoredPosition.x,
            y = listTransform.sizeDelta.y + itemsHeight * 0.5f
        };

        for (int i = 1; i < items.Length; i++)
        {
            DropdownItem item = items[i];
            RectTransform itemTransform = (RectTransform)item.transform;

            itemTransform.anchoredPosition = new Vector2
            {
                x = itemTransform.anchoredPosition.x,
                y = listTransform.sizeDelta.y - itemsHeight * (i - 1) - itemsHeight * 0.5f
            };
        }
    }

    private void SelectSearchField()
    {
        int backupSelectionAnchorPosition = _searchField.selectionAnchorPosition;
        int backupSelectionStringAnchorPosition = _searchField.selectionStringAnchorPosition;
        {
            _searchField.Select();
        }
        _searchField.selectionAnchorPosition = backupSelectionAnchorPosition;
        _searchField.selectionStringAnchorPosition = backupSelectionStringAnchorPosition;
    }

    private DropdownItem[] GetItems(bool includeInactive)
    {
        return GetComponentsInChildren<DropdownItem>(includeInactive);
    }

    private string TextToTerm(string text)
    {
        return text.ToLower().Trim();
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