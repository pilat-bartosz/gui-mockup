using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class GUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ListElement _listElementPrefab;
    [SerializeField] private Transform _listParentObject;
    [SerializeField] private Button _listPreviousButton;
    [SerializeField] private Button _listNextButton;

    [SerializeField] private GameObject _loadingScreen;

    private int _maxElementCount;

    private List<ListElement> _listElements;

    public void Initialize(int elementCountPerPage,
        Action onPreviousPageButton,
        Action onNextPageButton)
    {
        _maxElementCount = elementCountPerPage;

        //initialize list
        Assert.IsNotNull(_listElementPrefab);
        Assert.IsNotNull(_listParentObject);
        _listElements = new List<ListElement>(_maxElementCount);
        for (var i = 0; i < _maxElementCount; i++)
        {
            var newElement = Instantiate(_listElementPrefab, _listParentObject);
            newElement.Disable();
            _listElements.Add(newElement);
        }

        _listPreviousButton.onClick.RemoveAllListeners();
        _listPreviousButton.onClick.AddListener(() => onPreviousPageButton());

        _listNextButton.onClick.RemoveAllListeners();
        _listNextButton.onClick.AddListener(() => onNextPageButton());
    }

    public void UpdateButtonsInteractable(bool previousButtonState, bool nextButtonState)
    {
        _listPreviousButton.interactable = previousButtonState;
        _listNextButton.interactable = nextButtonState;
    }

    /// <summary>
    /// Updates GUI with new data
    /// TODO add animations? leantween?
    /// </summary>
    /// <param name="newItems">Set of new data. Shouldn't have more items than a page.</param>
    public void UpdateGUI(IList<DataItem> newItems)
    {
        Assert.IsTrue(newItems.Count <= _maxElementCount,
            $"UpdateGUI - items count (newItems.Count) was greater than page max ({_maxElementCount})"
        );

        //TODO Animate out

        //Setup new data
        for (var i = 0; i < _maxElementCount; i++)
        {
            var element = _listElements[i];
            Assert.IsNotNull(element, $"element {i} in the GUI list was null");

            if (i < newItems.Count)
            {
                var newItem = newItems[i];
                Assert.IsNotNull(newItem, $"new item at index {i} was null");

                element.UpdateElement(newItem.Category,
                    (i + 1).ToString(),
                    newItem.Description,
                    newItem.Special
                );
            }
            else
            {
                element.Disable();
            }
        }

        //TODO Animate in
    }

    public void ToggleLoadingScreenTo(bool state)
    {
        _loadingScreen.SetActive(state);
    }
}