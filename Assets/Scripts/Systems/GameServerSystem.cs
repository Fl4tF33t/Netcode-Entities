using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct GameServerSystem : ISystem {
    private const float GRID_SIZE = 3.1f;

    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        state.RequireForUpdate<EntitiesReferences>();
        state.RequireForUpdate<GameServerData>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
        EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        // Handle the start of the game
        {
            RefRW<GameServerData> gameServerData = SystemAPI.GetSingletonRW<GameServerData>();
            if (gameServerData.ValueRO.state == GameServerData.State.WaitingForPlayers) {
                EntityQuery networkStreamInGameEntityQuery = state.EntityManager.CreateEntityQuery(typeof(NetworkStreamInGame));
                if (networkStreamInGameEntityQuery.CalculateEntityCount() == 2) {
                   UnityEngine.Debug.Log("Gameshould start");
                    gameServerData.ValueRW.state = GameServerData.State.InGame;
                    gameServerData.ValueRW.currentPlayablePlayerType = PlayerType.Cross;

                    Entity gameStartedRpc = ecb.CreateEntity();
                    ecb.AddComponent<GameStartedRpc>(gameStartedRpc);
                    ecb.AddComponent<SendRpcCommandRequest>(gameStartedRpc);
                }
            }
        }


        // Handle the ClickedOnGridPositionRpc
        foreach ((
            RefRO<ClickedOnGridPositionRpc> clickedOnGridPositionRpc,
            RefRO<ReceiveRpcCommandRequest> receiveRpcCommandRequest,
            Entity entity
            )
            in SystemAPI.Query<
                RefRO<ClickedOnGridPositionRpc>,
                RefRO<ReceiveRpcCommandRequest>>()
                    .WithEntityAccess()) {

            ecb.DestroyEntity(entity);

            RefRW<GameServerData> gameServerData = SystemAPI.GetSingletonRW<GameServerData>();
            if (gameServerData.ValueRO.currentPlayablePlayerType != clickedOnGridPositionRpc.ValueRO.playerType) {
                continue;
            }

            Entity playerEntityPrefab = Entity.Null;
            switch (clickedOnGridPositionRpc.ValueRO.playerType) {
                default:
                case PlayerType.Cross:
                    playerEntityPrefab = entitiesReferences.crossPrefabEntity;
                    gameServerData.ValueRW.currentPlayablePlayerType = PlayerType.Circle;
                    break;
                case PlayerType.Circle:
                    playerEntityPrefab = entitiesReferences.circlePrefabEntity;
                    gameServerData.ValueRW.currentPlayablePlayerType = PlayerType.Cross;
                    break;
            }

            Entity spawnedEntity = ecb.Instantiate(playerEntityPrefab);
            float3 worldPosition = GetWorldPosition(clickedOnGridPositionRpc.ValueRO.x, clickedOnGridPositionRpc.ValueRO.y);
            ecb.SetComponent(spawnedEntity, LocalTransform.FromPosition(worldPosition));
        }
    }

    private float3 GetWorldPosition(int x, int y) {
        return new float3(
            -GRID_SIZE + (x * GRID_SIZE),
            -GRID_SIZE + (y * GRID_SIZE),
            0
        );
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) {

    }
}
