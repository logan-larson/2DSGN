using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameTimer : NetworkBehaviour
{
    [SerializeField]
    TMP_Text _text;

    private float _time = 0;
    private bool _isRunning = false;

    private void Awake()
    {
        _text.text = string.Format("{0}:{1}", Mathf.Floor(_time / 60).ToString("00"), Mathf.Floor(_time % 60).ToString("00"));
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        GameStateManager.Instance.OnGameStart.AddListener(StartTimerObserversRpc);
        GameStateManager.Instance.OnGameEnd.AddListener(StopTimerObserversRpc);
    }

    [ObserversRpc]
    private void StartTimerObserversRpc()
    {
        StartTimer();
    }

    [ObserversRpc]
    private void StopTimerObserversRpc()
    {
        StopTimer();
    }

    private void StartTimer()
    {
        _time = 0f;
        _isRunning = true;
    }

    private void StopTimer()
    {
        _isRunning = false;
    }

    private void Update()
    {
        if (_isRunning)
        {
            _time += Time.deltaTime;

            // Format like this: 00:00
            _text.text = string.Format("{0}:{1}", Mathf.Floor(_time / 60).ToString("00"), Mathf.Floor(_time % 60).ToString("00"));
        }   
    }
}
