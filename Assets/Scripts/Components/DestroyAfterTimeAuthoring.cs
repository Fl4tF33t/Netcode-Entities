using Unity.Entities;
using UnityEngine;

public class DestroyAfterTimeAuthoring : MonoBehaviour {

    [SerializeField]
    private float timer = 5f;

    public class Baker : Baker<DestroyAfterTimeAuthoring> {
        public override void Bake(DestroyAfterTimeAuthoring authoring) {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new DestroyAfterTime {
                timer = authoring.timer
            });
        }
    }
}

public struct DestroyAfterTime : IComponentData {
    public float timer;
}
