using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class GameServerDataAuthoring : MonoBehaviour {
    
    public class Baker : Baker<GameServerDataAuthoring> {
        public override void Bake(GameServerDataAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent<GameServerData>(entity);
        }
    }
}

public struct GameServerData : IComponentData {
    public enum State {
        WaitingForPlayers,
        InGame,
    }
    public State state;

    [GhostField]
    public PlayerType currentPlayablePlayerType;
    [GhostField]
    public int playerCrossScore;
    [GhostField]
    public int playerCircleScore;
}

public struct GameServerDataArrays : IComponentData {
    public NativeArray<PlayerType> playerTypesArray;
    public NativeArray<Line> lineArray;
}
