using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "Movement Scriptable", menuName = "Data/Movement variables")]
    public class MovementSettings : ScriptableObject
    {
        [Range(0f, 100f)]
        public float BaseSpeed = 5f;
        [Range(0f, 100f)]
        public float AccelerationForce = 30f;
        [Range(0f, 100f)]
        public float InertiaDamping = 10f;
    }

}