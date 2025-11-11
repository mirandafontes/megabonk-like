using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Movement Scriptable", menuName = "Data/Movement variables")]
    public class MovementSettings : ScriptableObject
    {
        [Range(0.1f, 20f)]
        public float MaxSpeed = 5f;
        [Range(0f, 100f)]
        public float AccelerationForce = 30f;
        [Range(0f, 100f)]
        public float InertiaDamping = 10f;
    }

}