using ScriptableObjects;
using UnityEngine;

namespace Enemy
{
    /// <summary>
    /// EnemyData representa o estado atual do inimigo.
    /// É uma classe com dados mutáveis
    /// e um blueprint de referência.
    /// Algumas variáveis são expostas para facilitar debbuging e visualização.
    /// </summary>
    [System.Serializable]
    public class EnemyData
    {
        [SerializeField]
        private EnemyBlueprint blueprint;

        public string CurrentName { get; set; }
        public float CurrentHealth { get; set; }
        public float CurrentSpeed { get; set; }
        public Vector3 Position { get; set; }
        public EnemyType EnemyType { get; set; }
        public bool IsAvoiding { get; set; }
        public bool IsDying { get; set; }
        public PursuitStrategy PursuitStrategy { get; set; }
        public bool IsKnockingBack { get; set; }
        public int Experience {get; set;}

        public EnemyData(EnemyBlueprint newBlueprint, Vector3 spawnPosition)
        {
            blueprint = newBlueprint;

            CurrentName = blueprint.EnemyName;
            CurrentHealth = blueprint.BaseHealth;
            CurrentSpeed = blueprint.MovementSettings.BaseSpeed;
            EnemyType = blueprint.EnemyType;
            PursuitStrategy = blueprint.PursuitStrategy;
            Experience = blueprint.ExpOnDeath;
            IsAvoiding = false;
            IsDying = false;
            IsKnockingBack = false;

            Position = spawnPosition;
        }

        public void NewEnemyData(EnemyBlueprint newBlueprint, Vector3 spawnPosition)
        {
            blueprint = newBlueprint;

            CurrentName = blueprint.EnemyName;
            CurrentHealth = blueprint.BaseHealth;
            CurrentSpeed = blueprint.MovementSettings.BaseSpeed;
            EnemyType = blueprint.EnemyType;
            PursuitStrategy = blueprint.PursuitStrategy;
            Experience = blueprint.ExpOnDeath;
            IsAvoiding = false;
            IsDying = false;
            IsKnockingBack = false;

            Position = spawnPosition;
        }
    }
}