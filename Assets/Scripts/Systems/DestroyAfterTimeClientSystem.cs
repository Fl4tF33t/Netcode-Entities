using Unity.Burst;
using Unity.Entities;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
partial struct DestroyAfterTimeClientSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        foreach ((
            RefRW<DestroyAfterTime> destroyAfterTime, 
            Entity entity) 
            in SystemAPI.Query<
                RefRW<DestroyAfterTime>>()
                    .WithEntityAccess()) {

            destroyAfterTime.ValueRW.timer -= SystemAPI.Time.DeltaTime;
            if (destroyAfterTime.ValueRW.timer <= 0) {
                ecb.DestroyEntity(entity);
            }
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
