using Unity.Entities;
using UnityEngine;

public class SpawnedObjectAuthoring : MonoBehaviour {
    
    public class Baker : Baker<SpawnedObjectAuthoring> {
        public override void Bake(SpawnedObjectAuthoring authoring) {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<SpawnedObject>(entity);
        }
    }
}

public struct SpawnedObject : IComponentData { }
