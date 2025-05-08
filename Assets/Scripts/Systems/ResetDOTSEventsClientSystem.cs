using Unity.Burst;
using Unity.Entities;

[UpdateInGroup(typeof(LateSimulationSystemGroup), OrderLast = true)]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
partial struct ResetDOTSEventsClientSystem : ISystem {
    //[BurstCompile]
    public void OnUpdate(ref SystemState state) {
        EntityCommandBuffer ecs = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        foreach ((
            RefRO<OnConnectedEvent> onConnectedEvent,
            Entity entity)
                in SystemAPI.Query<
                    RefRO<OnConnectedEvent>>()
                        .WithEntityAccess()) {

            ecs.DestroyEntity(entity);
            DOTSEventsMonobehaviour.Instance.TriggerOnClientConnected(onConnectedEvent.ValueRO.connectionId);
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) {

    }
}
