using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct GoInGameServerSystem : ISystem {
    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<GoInGameRequestRpc>()
            .WithAll<ReceiveRpcCommandRequest>()
            .Build(ref state);

        state.RequireForUpdate(entityQuery);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);
        foreach ((
            RefRO<ReceiveRpcCommandRequest> receiveRpcCommandRequest,
            Entity entity
            )
            in SystemAPI.Query<
                RefRO<ReceiveRpcCommandRequest>>()
                    .WithAll<GoInGameRequestRpc>()
                    .WithEntityAccess()) {

            ecb.AddComponent<NetworkStreamInGame>(receiveRpcCommandRequest.ValueRO.SourceConnection);
            ecb.DestroyEntity(entity);
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) {

    }
}
