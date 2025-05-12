using System;
using TMPro;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour {
    [SerializeField]
    private TextMeshProUGUI resultsTextMesh;
    [SerializeField]
    private Color winColor;
    [SerializeField]
    private Color loseColor;
    [SerializeField] 
    private Color drawColor;
    [SerializeField]
    private Button rematchButton;

    private void Start() {
        Hide();
        rematchButton.onClick.AddListener(() => {
            EntityManager entityManager = ClientServerBootstrap.ClientWorld.EntityManager;
            entityManager.CreateEntity(typeof(RematchRpc), typeof(SendRpcCommandRequest));
        });
        DOTSEventsMonobehaviour.Instance.OnGameOver += DOTSEventsMonobehaviour_OnGameOver;
        DOTSEventsMonobehaviour.Instance.OnRematch += DOTSEventsMonobehaviour_OnRematch;
        DOTSEventsMonobehaviour.Instance.OnGameDraw += DOTSEventsMonobehaviour_OnGameDraw;
    }

    private void DOTSEventsMonobehaviour_OnGameDraw() {
        resultsTextMesh.color = drawColor;
        resultsTextMesh.text = "Game Draw!";
        Show();
    }

    private void DOTSEventsMonobehaviour_OnRematch() {
        Hide();
    }

    private void DOTSEventsMonobehaviour_OnGameOver(PlayerType winPlayerType) {
        EntityManager entityManager = ClientServerBootstrap.ClientWorld.EntityManager;
        EntityQuery gameClientDataEntityQuery = entityManager.CreateEntityQuery(typeof(GameClientData));
        GameClientData gameClientData = gameClientDataEntityQuery.GetSingleton<GameClientData>();
        if (winPlayerType == gameClientData.localPlayerType) {
            resultsTextMesh.color = winColor;
            resultsTextMesh.text = "You Win!";
        } else {
            resultsTextMesh.color = loseColor;
            resultsTextMesh.text = "You Lose!";
        }

        Show();
    }

    private void Show() {
        gameObject.SetActive(true);
    }

    private void Hide() {
        gameObject.SetActive(false);
    }
}
