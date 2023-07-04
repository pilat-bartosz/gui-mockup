using UnityEngine;
using UnityEngine.Assertions;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GUIManager _guiManager;
    [SerializeField] private ServerConnectionHandler _serverConnectionHandler;

    [Tooltip("Variable that dictates how many items a page would have")]
    [SerializeField] private int _pageMaxElementCount = 5;

    //should be in range 0-_maxPage inclusive
    private int _currentPageIndex = 0;
    private int _maxPageIndex;

    void Awake()
    {
        Assert.IsNotNull(_guiManager);
        Assert.IsNotNull(_serverConnectionHandler);
    }

    private async void Start()
    {
        //Make sure buttons are disabled during the loading
        _guiManager.UpdateButtonsInteractable(false, false);

        //Initialize server and get the maximum number of pages
        var dataTask = _serverConnectionHandler.Initialize(_pageMaxElementCount);
        await dataTask;
        _maxPageIndex = dataTask.Result;

        //Initialize gui
        _guiManager.Initialize(_pageMaxElementCount, OnPreviousPageButtonClicked, OnNextPageButtonClicked);

        //Turn to the first page
        TurnPageTo(0);
    }

    private void OnPreviousPageButtonClicked()
    {
        TurnPage(-1);
    }

    private void OnNextPageButtonClicked()
    {
        TurnPage(1);
    }


    private void TurnPage(int value)
    {
        TurnPageTo(_currentPageIndex + value);
    }

    private void TurnPageTo(int pageNumber)
    {
        Debug.Log($"Turning page to {pageNumber}");
        
        //Show loading screen
        _guiManager.ToggleLoadingScreenTo(true);

        //Make sure buttons are disabled during the loading
        _guiManager.UpdateButtonsInteractable(false, false);

        _currentPageIndex = pageNumber;

        _serverConnectionHandler.GetDataForPage(
            _currentPageIndex,
            data =>
            {
                //Update GUI with recieved data
                _guiManager.UpdateGUI(data);

                //Turn buttons back on
                _guiManager.UpdateButtonsInteractable(_currentPageIndex > 0, _currentPageIndex < _maxPageIndex);

                //Hide loading screen
                _guiManager.ToggleLoadingScreenTo(false);
            });
    }
}