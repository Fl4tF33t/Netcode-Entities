using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
[UpdateAfter(typeof(GoInGameClientSystem))]
partial struct GameClientSystem : ISystem {
    [BurstCompile]
    public void OnCreate(ref SystemState state) {

    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {

        EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();

        // Handle the OnConnectedEvent
        foreach (
            RefRO<OnConnectedEvent> onConnectedEvent
            in SystemAPI.Query<
                RefRO<OnConnectedEvent>>()) {

            RefRW<GameClientData> gameClientData = SystemAPI.GetSingletonRW<GameClientData>();
            if (onConnectedEvent.ValueRO.connectionId == 1) {
                gameClientData.ValueRW.localPlayerType = PlayerType.Cross;
            } else {
                gameClientData.ValueRW.localPlayerType = PlayerType.Circle;
            }
        }

        // Handle the GameStartedRpc
        foreach ((
            RefRO<GameStartedRpc> gameStartedRpc,
            RefRO<ReceiveRpcCommandRequest> receiveRpcCommandRequest,
            Entity entity
            )
            in SystemAPI.Query<
                RefRO<GameStartedRpc>,
                RefRO<ReceiveRpcCommandRequest>>()
                    .WithEntityAccess()) {

            ecb.DestroyEntity(entity);
            DOTSEventsMonobehaviour.Instance.TriggerOnGameStarted();
        }

        // Handle winning/end of Game
        foreach ((
            RefRO<GameWinRpc> gameWinRpc,
            RefRO<ReceiveRpcCommandRequest> recieveRpcCommandRequest,
            Entity entity)
            in SystemAPI.Query<
                RefRO<GameWinRpc>,
                RefRO<ReceiveRpcCommandRequest>>()
                    .WithEntityAccess()) {

            DOTSEventsMonobehaviour.Instance.TriggerOnGameOver(gameWinRpc.ValueRO.winPlayerType);
            GameClientData gameClientData = SystemAPI.GetSingleton<GameClientData>();

            if (gameWinRpc.ValueRO.winPlayerType == gameClientData.localPlayerType) {
                ecb.Instantiate(entitiesReferences.winSfxEntity);
            } else {
                ecb.Instantiate(entitiesReferences.loseSfxEntity);
            }

            ecb.DestroyEntity(entity);
        }

        // Handle Rematch of Game
        foreach ((
            RefRO<RematchRpc> rematchRpc,
            RefRO<ReceiveRpcCommandRequest> recieveRpcCommandRequest,
            Entity entity)
            in SystemAPI.Query<
                RefRO<RematchRpc>,
                RefRO<ReceiveRpcCommandRequest>>()
                    .WithEntityAccess()) {

            DOTSEventsMonobehaviour.Instance.TriggerOnRematch();
            ecb.DestroyEntity(entity);
        }

        // Handle draw of Game
        foreach ((
            RefRO<GameDrawRpc> drawRpc,
            RefRO<ReceiveRpcCommandRequest> recieveRpcCommandRequest,
            Entity entity)
            in SystemAPI.Query<
                RefRO<GameDrawRpc>,
                RefRO<ReceiveRpcCommandRequest>>()
                    .WithEntityAccess()) {

            DOTSEventsMonobehaviour.Instance.TriggerOnGameDraw();
            ecb.DestroyEntity(entity);
        }

        // Handle sound effects
        foreach ((
            RefRO<ClickedOnGridPositionRpc> clickedOnGridPositionRpc,
            RefRO<ReceiveRpcCommandRequest> recieveRpcCommandRequest,
            Entity entity)
            in SystemAPI.Query<
                RefRO<ClickedOnGridPositionRpc>,
                RefRO<ReceiveRpcCommandRequest>>()
                    .WithEntityAccess()) {

            ecb.DestroyEntity(entity);
            ecb.Instantiate(entitiesReferences.placeSfxEntity);
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) {

    }
}
