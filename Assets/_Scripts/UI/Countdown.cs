using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Countdown : NetworkBehaviour
{
    [SerializeField]
    TMP_Text _text;
    [SerializeField]
    Image _image;

    private int _countdownTime = 5;

    private void Awake()
    {
        _text = GetComponentInChildren<TMP_Text>();
        _image = GetComponent<Image>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        _image.enabled = false;
        _text.enabled = false;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        GameStateManager.Instance.OnInitiateCountdown.AddListener(StartCountdownObserversRpc);
        GameStateManager.Instance.OnGameStart.AddListener(GameStartObserversRpc);
    }

    [ObserversRpc]
    private void StartCountdownObserversRpc()
    {
        StartCountdown();
    }

    [ObserversRpc]
    private void GameStartObserversRpc()
    {
        GameStart();
    }

    private void GameStart()
    {
        _image.enabled = false;

        StartCoroutine(GoNuts());
    }

    private IEnumerator GoNuts()
    {
        float timer = 2f;

        while (timer > 0f)
        {
            // Update the timer.
            timer -= Time.deltaTime;

            // Calculate the alpha value based on the remaining time.
            float alpha = Mathf.Lerp(0.0f, 1.0f, timer / 2f);

            _text.alpha = alpha; // Fade the text each countdown tick.

            yield return null; // Wait for the next frame.
        }

        // Countdown has finished.
        _text.enabled = false;
    }

    private void StartCountdown()
    {
        _image.enabled = true;
        _text.enabled = true;
        _text.fontSize = 100;

        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        float timer = _countdownTime;

        while (timer > 0f)
        {
            // Update the timer.
            timer -= Time.deltaTime;

            // Calculate the alpha value based on the remaining time.
            float alpha = Mathf.Lerp(0.0f, 1.0f, timer / (_countdownTime - 2f));
            // Fade the text each countdown tick.
            float textAlpha = timer - Mathf.FloorToInt(timer);

            // Update the color with the new alpha value.
            Color currentColor = new Color(0f, 0f, 0f, alpha);

            // Apply the color to your object's renderer.
            _image.color = currentColor;

            // Update the countdown text.
            _text.text = Mathf.CeilToInt(timer).ToString(); // Display the countdown time as an integer.
            _text.alpha = textAlpha; // Fade the text each countdown tick.

            yield return null; // Wait for the next frame.
        }

        // Countdown has finished.
        _text.text = "GO NUTS";
        _text.fontSize = 120;

        _image.enabled = false;
    }
}

