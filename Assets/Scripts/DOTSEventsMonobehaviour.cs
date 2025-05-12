using System;
using UnityEngine;

public class DOTSEventsMonobehaviour : MonoBehaviour {

    public static DOTSEventsMonobehaviour Instance { get; private set; }

    public event Action<int> OnClientConnected;
    public event Action OnGameStarted;
    public event Action<PlayerType> OnGameOver;
    public event Action OnRematch;
    public event Action OnGameDraw;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    public void TriggerOnClientConnected(int connectionId) => OnClientConnected?.Invoke(connectionId);
    public void TriggerOnGameStarted() => OnGameStarted?.Invoke();
    public void TriggerOnGameOver(PlayerType winPlayerType) => OnGameOver?.Invoke(winPlayerType);
    public void TriggerOnRematch() => OnRematch?.Invoke();
    public void TriggerOnGameDraw() => OnGameDraw?.Invoke();

}
