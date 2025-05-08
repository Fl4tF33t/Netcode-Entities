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
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) {

    }
}
