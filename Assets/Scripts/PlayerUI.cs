using System;
using TMPro;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class PlayerUI : MonoBehaviour {

    [SerializeField]
    private GameObject crossArrowGameObject;
    [SerializeField]
    private GameObject circleArrowGameObject;
    [SerializeField]
    private GameObject crossYouGameObject;
    [SerializeField]
    private GameObject circleYouGameObject;
    [SerializeField]
    private TextMeshProUGUI playerCrossScore;
    [SerializeField]
    private TextMeshProUGUI playerCircleScore;


    private void Awake() {
        crossArrowGameObject.SetActive(false);
        circleArrowGameObject.SetActive(false);
        crossYouGameObject.SetActive(false);
        circleYouGameObject.SetActive(false);
        playerCrossScore.text = "";
        playerCircleScore.text = "";
    }

    private void Start() {
        DOTSEventsMonobehaviour.Instance.OnGameStarted += OnGameStarted;
    }

    private void OnGameStarted() {
        EntityManager entityManager = ClientServerBootstrap.ClientWorld.EntityManager;
        EntityQuery gameClientDataEntityQuery = entityManager.CreateEntityQuery(typeof(GameClientData));
        if (!gameClientDataEntityQuery.HasSingleton<GameServerData>()) {
            return;
        }
        GameClientData gameClientData = gameClientDataEntityQuery.GetSingleton<GameClientData>();

        if (gameClientData.localPlayerType == PlayerType.Cross) {
            crossYouGameObject.SetActive(true);
        } else {
            circleYouGameObject.SetActive(true);
        }
    }

    private void Update() {
        UpdateCurrentPlayableArrow();
        UpdateScore();
    }

    private void UpdateCurrentPlayableArrow() {
        EntityManager entityManager = ClientServerBootstrap.ClientWorld.EntityManager;
        EntityQuery gameServerDataEntityQuery = entityManager.CreateEntityQuery(typeof(GameServerData));
        if (!gameServerDataEntityQuery.HasSingleton<GameServerData>()) {
            return;
        }
        GameServerData gameServerData = gameServerDataEntityQuery.GetSingleton<GameServerData>();

        if (gameServerData.currentPlayablePlayerType == PlayerType.Cross) {
            crossArrowGameObject.SetActive(true);
            circleArrowGameObject.SetActive(false);
        } else {
            crossArrowGameObject.SetActive(false);
            circleArrowGameObject.SetActive(true);
        }
    }

    private void UpdateScore() {
        EntityManager entityManager = ClientServerBootstrap.ClientWorld.EntityManager;
        EntityQuery gameServerDataEntityQuery = entityManager.CreateEntityQuery(typeof(GameServerData));
        if (!gameServerDataEntityQuery.HasSingleton<GameServerData>()) {
            return;
        }
        GameServerData gameServerData = gameServerDataEntityQuery.GetSingleton<GameServerData>();

        playerCrossScore.text = gameServerData.playerCrossScore.ToString();
        playerCircleScore.text = gameServerData.playerCircleScore.ToString();
    }
}
