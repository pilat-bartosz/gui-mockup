using UnityEngine;
using UnityEngine.Assertions;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GUIManager _guiManager;
    [SerializeField] private ServerConnectionHandler _serverConnectionHandler;

    [Tooltip("Variable that dictates how many items a page would have")]
    [SerializeField] private int _pageMaxElementCount = 5;

    private int _currentPage = 0;
    private int _maxPage;

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
        _maxPage = dataTask.Result;
        Debug.Log(_maxPage);

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
        TurnPageTo(_currentPage + value);
    }

    private void TurnPageTo(int value)
    {
        //Show loading screen
        _guiManager.ToggleLoadingScreenTo(true);

        //Make sure buttons are disabled during the loading
        _guiManager.UpdateButtonsInteractable(false, false);

        _currentPage = value;

        _serverConnectionHandler.GetDataForPage(
            _currentPage,
            data =>
            {
                //Update GUI with recieved data
                _guiManager.UpdateGUI(data);

                //Turn buttons back on
                _guiManager.UpdateButtonsInteractable(_currentPage > 0, _currentPage < _maxPage);

                //Hide loading screen
                _guiManager.ToggleLoadingScreenTo(false);
            });
    }
}