using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

public class ServerConnectionHandler : MonoBehaviour
{
    private DataServerMock _dataServerMock;

    private CancellationTokenSource _cancellationTokenSource;

    private int _dataCount;

    private static readonly int GUIListMax = 5;

    private DataItem[] _dataItems;
    private bool[] _dataAvailability;

    private async void Start()
    {
        //Show loading screen


        //Establish connection with the data server
        MockServerConnection();

        Assert.IsNotNull(_dataServerMock, "DataServer connection not established");

        _cancellationTokenSource = new CancellationTokenSource();

        //Get available data count
        var dataCountTask = _dataServerMock.DataAvailable(_cancellationTokenSource.Token);
        await dataCountTask;
        _dataCount = dataCountTask.Result;

        _dataItems = new DataItem[_dataCount];
        var dataSetsCount = Mathf.CeilToInt((float)_dataCount / GUIListMax);

        _dataAvailability = new bool[dataSetsCount];

        //Get first batch of the data
        RequestNewData(
            0,
            Mathf.Min(_dataCount, GUIListMax),
            _cancellationTokenSource.Token, data =>
            {
                //Copy data
                data.CopyTo(_dataItems, 0);

                //Mark data available
                _dataAvailability[0] = true;

                //Fill the GUI with the data

                //Disable loading screen

                //TODO remove:
                Debug.Log(_dataCount);
            }
        );
    }

    private void Update()
    {
        //TODO remove
        if (Input.anyKeyDown && _dataAvailability[1] == false)
        {
            RequestNewData(
                5,
                Mathf.Min(_dataCount, GUIListMax),
                _cancellationTokenSource.Token, data =>
                {
                    //Copy data
                    data.CopyTo(_dataItems, 5);

                    //Mark data available
                    _dataAvailability[1] = true;

                    //Fill the GUI with the data

                    //Disable loading screen

                    //TODO remove:
                    Debug.Log(_dataCount);
                }
            );
        }
    }

    private void MockServerConnection()
    {
        _dataServerMock = new DataServerMock();
    }

    private async void RequestNewData(int startIndex,
        int length,
        CancellationToken cancellationToken,
        Action<IList<DataItem>> callback)
    {
        var dataTask = _dataServerMock.RequestData(
            startIndex,
            length,
            _cancellationTokenSource.Token);
        await dataTask;
        callback.Invoke(dataTask.Result);
    }
}