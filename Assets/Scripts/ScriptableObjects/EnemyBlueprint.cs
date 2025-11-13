using Enemy;
using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "Data/Enemy Blueprint")]
    public class EnemyBlueprint : ScriptableObject
    {
        [Header("- Identification -")]
        public EnemyType EnemyType;
        public string EnemyName = "Here's come a new challenger";

        [Header("- Movement -")]
        public MovementSettings MovementSettings;

        [Header("- Stats Base -")]
        [Range(1f, 100f)]
        public float BaseHealth = 10.0f;
        [Range(1f, 10f)]
        public float BaseDamage = 1.0f;
    }
}