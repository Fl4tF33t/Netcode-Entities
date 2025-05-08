using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using Unity.Physics;
using Unity.Mathematics;
using Unity.NetCode;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
partial struct GridPositionClickClientSystem : ISystem {
    [BurstCompile]
    public void OnCreate(ref SystemState state) {

    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state) {
        EntityCommandBuffer ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        if (Input.GetMouseButtonDown(0)) {
            PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;

            float3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            if (collisionWorld.CastRay(new RaycastInput {
                    Start = mouseWorldPosition,
                    End = mouseWorldPosition + new float3(0, 0, 100),
                    Filter = CollisionFilter.Default
                }, out Unity.Physics.RaycastHit raycastHit)) {

                if (SystemAPI.HasComponent<GridPosition>(raycastHit.Entity)) {
                    GridPosition gridPosition = SystemAPI.GetComponent<GridPosition>(raycastHit.Entity);

                    GameClientData gameClientData = SystemAPI.GetSingleton<GameClientData>();
                    Entity rpcEntity = ecb.CreateEntity();
                    ecb.AddComponent(rpcEntity, new ClickedOnGridPositionRpc {
                        x = gridPosition.x,
                        y = gridPosition.y,
                        playerType = gameClientData.localPlayerType
                    });
                    ecb.AddComponent<SendRpcCommandRequest>(rpcEntity);
                }
            }
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) {

    }
}

public struct ClickedOnGridPositionRpc : IRpcCommand {
    public int x;
    public int y;
    public PlayerType playerType;
}
