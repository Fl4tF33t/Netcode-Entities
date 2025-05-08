using System;
using UnityEngine;

public class DOTSEventsMonobehaviour : MonoBehaviour {

    public static DOTSEventsMonobehaviour Instance { get; private set; }

    public event Action<int> OnClientConnected;
    public event Action OnGameStarted;

    private void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }
    }

    public void TriggerOnClientConnected(int connectionId) => OnClientConnected?.Invoke(connectionId);
    public void TriggerOnGameStarted() => OnGameStarted?.Invoke();
    
}
