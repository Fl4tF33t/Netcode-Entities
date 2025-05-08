using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
partial struct GoInGameClientSystem : ISystem {
    [BurstCompile]
    public void OnCreate(ref SystemState state) {
        EntityQuery entityQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<NetworkId>()
            .WithNone<NetworkStreamInGame>()
            .Build(ref state);

        state.RequireForUpdate(entityQuery);
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);
        foreach ((
            RefRO<NetworkId> networkId,
            Entity entity
            )
            in SystemAPI.Query<
                RefRO<NetworkId>>()
                    .WithNone<NetworkStreamInGame>()
                    .WithEntityAccess()) {
            
            ecb.AddComponent<NetworkStreamInGame>(entity);

            Entity rpcEntity = ecb.CreateEntity();
            ecb.AddComponent<GoInGameRequestRpc>(rpcEntity);
            ecb.AddComponent<SendRpcCommandRequest>(rpcEntity);
            
            Entity onConnectedEventEntity = ecb.CreateEntity();
            ecb.AddComponent(onConnectedEventEntity, new OnConnectedEvent {
                connectionId = networkId.ValueRO.Value
            });
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) {

    }
}

public struct GoInGameRequestRpc : IRpcCommand {
}
