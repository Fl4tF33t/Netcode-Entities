using System;
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

    private void Awake() {
        crossArrowGameObject.SetActive(false);
        circleArrowGameObject.SetActive(false);
        crossYouGameObject.SetActive(false);
        circleYouGameObject.SetActive(false);
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
    }

    private void UpdateCurrentPlayableArrow() {
        EntityManager entityManager = ClientServerBootstrap.ClientWorld.EntityManager;
        EntityQuery gameServerDataEntityQuery = entityManager.CreateEntityQuery(typeof(GameServerData));
        GameServerData gameServerData = gameServerDataEntityQuery.GetSingleton<GameServerData>();

        if (gameServerData.currentPlayablePlayerType == PlayerType.Cross) {
            crossArrowGameObject.SetActive(true);
            circleArrowGameObject.SetActive(false);
        } else {
            crossArrowGameObject.SetActive(false);
            circleArrowGameObject.SetActive(true);
        }
    }
}
