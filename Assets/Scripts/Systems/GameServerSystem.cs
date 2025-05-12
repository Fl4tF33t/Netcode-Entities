using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct GameServerSystem : ISystem {

    private const float GRID_SIZE = 3.1f;
    private const int GRID_WIDTH_HEIGHT = 3;

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

                    // Greate grids and arrays
                    Entity gameServerEntity = SystemAPI.GetSingletonEntity<GameServerData>();
                    NativeArray<Line> lineArray = new NativeArray<Line>(8, Allocator.Persistent);
                    lineArray[0] = new Line {
                        a = new int2(0, 0),
                        b = new int2(1, 0),
                        c = new int2(2, 0),
                        orientation = Line.Orientation.Horizontal
                    };
                    lineArray[1] = new Line {
                        a = new int2(0, 1),
                        b = new int2(1, 1),
                        c = new int2(2, 1),
                        orientation = Line.Orientation.Horizontal
                    };
                    lineArray[2] = new Line {
                        a = new int2(0, 2),
                        b = new int2(1, 2),
                        c = new int2(2, 2),
                        orientation = Line.Orientation.Horizontal
                    };
                    lineArray[3] = new Line {
                        a = new int2(0, 0),
                        b = new int2(0, 1),
                        c = new int2(0, 2),
                        orientation = Line.Orientation.Vertical
                    };
                    lineArray[4] = new Line {
                        a = new int2(1, 0),
                        b = new int2(1, 1),
                        c = new int2(1, 2),
                        orientation = Line.Orientation.Vertical
                    };
                    lineArray[5] = new Line {
                        a = new int2(2, 0),
                        b = new int2(2, 1),
                        c = new int2(2, 2),
                        orientation = Line.Orientation.Vertical
                    };
                    lineArray[6] = new Line {
                        a = new int2(0, 0),
                        b = new int2(1, 1),
                        c = new int2(2, 2),
                        orientation = Line.Orientation.DiagonalA
                    };
                    lineArray[7] = new Line {
                        a = new int2(0, 2),
                        b = new int2(1, 1),
                        c = new int2(2, 0),
                        orientation = Line.Orientation.DiagonalB
                    };

                    ecb.AddComponent(gameServerEntity, new GameServerDataArrays {
                        playerTypesArray = new NativeArray<PlayerType>(GRID_WIDTH_HEIGHT * GRID_WIDTH_HEIGHT, Allocator.Persistent),
                        lineArray = lineArray
                    });
                }
            }
        }


        // Handle the ClickedOnGridPositionRpc
        foreach ((
            RefRO<ClickedOnGridPositionRpc> clickedOnGridPositionRpc,
            RefRO<ReceiveRpcCommandRequest> receiveRpcCommandRequest,
            Entity entity)
            in SystemAPI.Query<
                RefRO<ClickedOnGridPositionRpc>,
                RefRO<ReceiveRpcCommandRequest>>()
                    .WithEntityAccess()) {

            ecb.DestroyEntity(entity);

            // check if it is the player's turn
            RefRW<GameServerData> gameServerData = SystemAPI.GetSingletonRW<GameServerData>();
            if (gameServerData.ValueRO.currentPlayablePlayerType != clickedOnGridPositionRpc.ValueRO.playerType) {
                continue;
            }

            // check if the clicked position is already occupied
            RefRW<GameServerDataArrays> gameServerDataArrays = SystemAPI.GetSingletonRW<GameServerDataArrays>();
            int flatIndex = GetFlatIndexFromGridPosition(clickedOnGridPositionRpc.ValueRO.x, clickedOnGridPositionRpc.ValueRO.y);
            if (gameServerDataArrays.ValueRO.playerTypesArray[flatIndex] != PlayerType.None) {
                continue;
            }
            gameServerDataArrays.ValueRW.playerTypesArray[flatIndex] = clickedOnGridPositionRpc.ValueRO.playerType;

            // spawn correct player entity, and switch turns
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

            Entity spawnSfx = ecb.CreateEntity();
            ecb.AddComponent(spawnSfx, clickedOnGridPositionRpc.ValueRO);
            ecb.AddComponent<SendRpcCommandRequest>(spawnSfx);

            TestWinner(gameServerDataArrays.ValueRO, gameServerData, ecb, entitiesReferences);
        }

        // check for rematch
        foreach ((
            RefRO<RematchRpc> rematchRpc,
            RefRO<ReceiveRpcCommandRequest> receiveRpcCommandRequest,
            Entity entity)
            in SystemAPI.Query<
                RefRO<RematchRpc>,      
                RefRO<ReceiveRpcCommandRequest>>()
                    .WithEntityAccess()) {

            ecb.DestroyEntity(entity);

            RefRW<GameServerDataArrays> gameServerDataArrays = SystemAPI.GetSingletonRW<GameServerDataArrays>();
            for (int i = 0; i < gameServerDataArrays.ValueRO.playerTypesArray.Length; i++) {
                gameServerDataArrays.ValueRW.playerTypesArray[i] = PlayerType.None;
            }

            RefRW<GameServerData> gameServerData = SystemAPI.GetSingletonRW<GameServerData>();
            gameServerData.ValueRW.currentPlayablePlayerType = PlayerType.Cross;

            foreach ((
                RefRO<SpawnedObject> spawnedObject,
                Entity spawnedEntity)
                in SystemAPI.Query<RefRO<SpawnedObject>>()
                    .WithEntityAccess()) {

                ecb.DestroyEntity(spawnedEntity);
            }

            Entity rematchRpcToCLient = ecb.CreateEntity();
            ecb.AddComponent<RematchRpc>(rematchRpcToCLient);
            ecb.AddComponent<SendRpcCommandRequest>(rematchRpcToCLient);
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state) {
        if (SystemAPI.HasSingleton<GameServerDataArrays>()) {
            GameServerDataArrays gameServerDataArrays = SystemAPI.GetSingleton<GameServerDataArrays>();
            gameServerDataArrays.playerTypesArray.Dispose();
            gameServerDataArrays.lineArray.Dispose();
        }
        
    }

    private float3 GetWorldPosition(int x, int y) {
        return new float3(
            -GRID_SIZE + (x * GRID_SIZE),
            -GRID_SIZE + (y * GRID_SIZE),
            0
        );
    }

    private int GetFlatIndexFromGridPosition(int x, int y) {
        return (y * GRID_WIDTH_HEIGHT) + x;
    }
    
    private bool TestWinnerLine(PlayerType aPlayerType, PlayerType bPlayerType, PlayerType cPlayerType) {
        return
            aPlayerType != PlayerType.None &&
            aPlayerType == bPlayerType && 
            bPlayerType == cPlayerType;
    }

    private void TestWinner(GameServerDataArrays gameServerDataArrays, RefRW<GameServerData> gameServerData, EntityCommandBuffer ecb, EntitiesReferences entitiesReferences) {

        foreach (Line line in gameServerDataArrays.lineArray) {
            if (TestWinnerLine(
                    gameServerDataArrays.playerTypesArray[GetFlatIndexFromGridPosition(line.a.x, line.a.y)],
                    gameServerDataArrays.playerTypesArray[GetFlatIndexFromGridPosition(line.b.x, line.b.y)],
                    gameServerDataArrays.playerTypesArray[GetFlatIndexFromGridPosition(line.c.x, line.c.y)]
                )) {
                gameServerData.ValueRW.currentPlayablePlayerType = PlayerType.None;

                Entity lineWinnerPrefabEntity = ecb.Instantiate(entitiesReferences.lineWinnerPrefabEntity);

                float3 worldPosition = GetWorldPosition(line.b.x, line.b.y);
                worldPosition.z = -1f;
                float eulerZ = 0f;
                switch (line.orientation) {
                    default:
                    case Line.Orientation.Horizontal: eulerZ = 0f; break;
                    case Line.Orientation.Vertical: eulerZ = 90f; break;
                    case Line.Orientation.DiagonalA: eulerZ = 45f; break;
                    case Line.Orientation.DiagonalB: eulerZ = -45f; break;
                }

                ecb.SetComponent(lineWinnerPrefabEntity, new LocalTransform {
                    Position = worldPosition,
                    Rotation = quaternion.RotateZ(eulerZ * math.TORADIANS),
                    Scale = 1f
                });

                Entity gameWinRpc = ecb.CreateEntity();
                ecb.AddComponent(gameWinRpc, new GameWinRpc {
                    winPlayerType = gameServerDataArrays.playerTypesArray[GetFlatIndexFromGridPosition(line.b.x, line.b.y)]
                });
                ecb.AddComponent<SendRpcCommandRequest>(gameWinRpc);

                switch (gameServerDataArrays.playerTypesArray[GetFlatIndexFromGridPosition(line.b.x, line.b.y)]) {
                    case PlayerType.Cross:
                        gameServerData.ValueRW.playerCrossScore++;
                        break;
                    case PlayerType.Circle:
                        gameServerData.ValueRW.playerCircleScore++;
                        break;
                }

                return;
            }
        }

        // check for draw
        bool isDraw = true;
        for (int i = 0; i < gameServerDataArrays.playerTypesArray.Length; i++) {
            if (gameServerDataArrays.playerTypesArray[i] == PlayerType.None) {
                isDraw = false;
                break;
            }
        }

        if (isDraw) {
            gameServerData.ValueRW.currentPlayablePlayerType = PlayerType.None;

            Entity gameDrawRpc = ecb.CreateEntity();
            ecb.AddComponent(gameDrawRpc, new GameDrawRpc());
            ecb.AddComponent<SendRpcCommandRequest>(gameDrawRpc);
        }
    } 
}
