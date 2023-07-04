using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

public class ServerConnectionHandler : MonoBehaviour
{
    [SerializeField] private bool _shouldRequestNearbyPages = true;

    private DataServerMock _dataServerMock;

    private CancellationTokenSource _cancellationTokenSource;

    private int _dataCount;
    private int _lastPageIndex;
    private int _pageSize;

    private DataItem[] _dataItems;
    private bool[] _dataPageAvailability;

    private void Awake()
    {
        //Establish connection with the data server
        MockServerConnection();

        Assert.IsNotNull(_dataServerMock, "DataServer connection not established");

        _cancellationTokenSource = new CancellationTokenSource();
    }

    /// <summary>
    /// Initialize server "data layout" based on page size.
    /// </summary>
    /// <param name="pageSize">Maximum number of elements per page</param>
    /// <returns>Index of last page</returns>
    public async Task<int> Initialize(int pageSize)
    {
        _pageSize = pageSize;

        //Get available data count
        var dataCountTask = _dataServerMock.DataAvailable(_cancellationTokenSource.Token);
        await dataCountTask;
        _dataCount = dataCountTask.Result;
        Debug.Log($"Data count {_dataCount}");

        _dataItems = new DataItem[_dataCount];
        var pageCount = Mathf.CeilToInt((float)_dataCount / pageSize);

        _dataPageAvailability = new bool[pageCount];
        _lastPageIndex = pageCount - 1;

        Debug.Log($"Last page index - {_lastPageIndex}");

        return _lastPageIndex;
    }

    /// <summary>
    /// Gets the data for a page.
    /// Additionally it will request the data for the next/previous page.
    /// </summary>
    /// <param name="pageIndex">Should be in range 0-_lastPageIndex</param>
    /// <param name="elementCount">Number of elements in the page</param>
    /// <param name="callback">Callback for that set of data</param>
    public void GetDataForPage(int pageIndex, Action<IList<DataItem>> callback)
    {
        Assert.IsTrue(CheckPageNumberRange(pageIndex),
            $"Page number: {pageIndex} in not between 0 and {_lastPageIndex}"
        );

        RequestDataForPage(pageIndex, callback);

        //check if there is data in cache for nearby pages already
        if (_shouldRequestNearbyPages)
        {
            var previousPage = pageIndex - 1;
            if (CheckPageNumberRange(previousPage))
            {
                RequestDataForPage(previousPage);
            }

            var nextPage = pageIndex + 1;
            if (CheckPageNumberRange(nextPage))
            {
                RequestDataForPage(nextPage);
            }
        }
    }

    /// <summary>
    /// Mock method to establish a connection to the server
    /// </summary>
    private void MockServerConnection()
    {
        _dataServerMock = new DataServerMock();
    }

    /// <summary>
    /// Requests data from the server.
    /// </summary>
    /// <param name="startIndex"></param>
    /// <param name="length"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="callback">Callback for requested data at the end</param>
    private async void RequestNewData(int startIndex,
        int length,
        CancellationToken cancellationToken,
        Action<IList<DataItem>> callback)
    {
        var dataTask = _dataServerMock.RequestData(
            startIndex,
            length,
            cancellationToken);
        await dataTask;
        callback.Invoke(dataTask.Result);
    }

    #region Helper_Methods

    private bool CheckPageNumberRange(int pageIndex)
    {
        return pageIndex >= 0 && pageIndex <= _lastPageIndex;
    }

    /// <summary>
    /// Gets existing data from cache or requests new data for a page
    /// </summary>
    /// <param name="pageNumber"></param>
    /// <param name="callback"></param>
    private void RequestDataForPage(int pageNumber, Action<IList<DataItem>> callback = null)
    {
        Assert.IsTrue(CheckPageNumberRange(pageNumber));

        //Debug.Log($"Requesting data for page {pageNumber}-{_dataPageAvailability[pageNumber]}");

        if (_dataPageAvailability[pageNumber])
        {
            callback?.Invoke(GetDataForPage(pageNumber));
        }
        else
        {
            RequestNewDataForPage(pageNumber, callback);
        }
    }

    private IList<DataItem> GetDataForPage(int pageIndex)
    {
        Assert.IsTrue(CheckPageNumberRange(pageIndex));
        Assert.IsNotNull(_dataItems);
        Assert.IsNotNull(_dataItems[pageIndex * _pageSize]);

        return _dataItems.ToList()
            .GetRange(pageIndex * _pageSize,
                pageIndex == _lastPageIndex ? _dataCount % _pageSize : _pageSize
            );
    }

    /// <summary>
    /// Requests new data from the server for a page.
    /// </summary>
    /// <param name="pageIndex"></param>
    /// <param name="callback"></param>
    private void RequestNewDataForPage(int pageIndex, Action<IList<DataItem>> callback)
    {
        Assert.IsTrue(CheckPageNumberRange(pageIndex));

        RequestNewData(
            pageIndex * _pageSize,
            pageIndex == _lastPageIndex ? _dataCount % _pageSize : _pageSize,
            _cancellationTokenSource.Token,
            data =>
            {
                //copy data to cache
                data.CopyTo(_dataItems, pageIndex * _pageSize);
                _dataPageAvailability[pageIndex] = true;

                //callback
                callback?.Invoke(data);
            }
        );
    }

    #endregion
}