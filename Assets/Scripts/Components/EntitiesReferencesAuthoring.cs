using Unity.Entities;
using UnityEngine;

public class EntitiesReferencesAuthoring : MonoBehaviour {
    [SerializeField]
    private Transform crossPrefabGameObj;
    [SerializeField]
    private Transform circlePrefabGameObj;
    [SerializeField]
    private Transform lineWinnerPrefabGameObj;
    [SerializeField]
    private Transform placeSfxPrefabGameObj;
    [SerializeField]
    private Transform winSfxPrefabGameObj;
    [SerializeField]
    private Transform loseSfxPrefabGameObj;

    public class Baker : Baker<EntitiesReferencesAuthoring> {
        public override void Bake(EntitiesReferencesAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EntitiesReferences {
                crossPrefabEntity = GetEntity(authoring.crossPrefabGameObj, TransformUsageFlags.Dynamic),
                circlePrefabEntity = GetEntity(authoring.circlePrefabGameObj, TransformUsageFlags.Dynamic),
                lineWinnerPrefabEntity = GetEntity(authoring.lineWinnerPrefabGameObj, TransformUsageFlags.Dynamic),
                placeSfxEntity = GetEntity(authoring.placeSfxPrefabGameObj, TransformUsageFlags.None),
                winSfxEntity = GetEntity(authoring.winSfxPrefabGameObj, TransformUsageFlags.None),
                loseSfxEntity = GetEntity(authoring.loseSfxPrefabGameObj, TransformUsageFlags.None)
            });
        }
    }
}

public struct EntitiesReferences : IComponentData {
    public Entity crossPrefabEntity;
    public Entity circlePrefabEntity;
    public Entity lineWinnerPrefabEntity;
    public Entity placeSfxEntity;
    public Entity winSfxEntity;
    public Entity loseSfxEntity;
}
