using System.Collections.Generic;
using System.Linq;
using Enemy;
using ScriptableObjects;
using UnityEngine;

namespace Wave
{
    [System.Serializable]
    public class WaveSetup
    {
        public List<SpawnGroup> Groups;

        [Tooltip("Tempo de cooldown pós wave")]
        [Range(0, 120)]
        public int CooldownTime;
        [Tooltip("Tempo antes de iniciar a wave")]
        [Range(0, 120)]
        public int InitalTimeBeforeSpawn = 10;

        [Tooltip("Inimigos máximos, independente de tipo, ao mesmo tempo, nessa wave")]
        public int MaxEnemies = 50;

        [Range(1, 240)]
        [Tooltip("Segundos")]
        public int TotalTime;

        /// <summary>
        /// Quantidade esperada de inimigos, somando todos os SpawnGroups
        /// </summary>
        public int ExpectedEnemies
        {
            get
            {
                if (Groups == null || Groups.Count <= 0)
                {
                    return 0;
                }

                return Groups.Sum(group => group.MaxEnemyCount);
            }
        }
    }

    [System.Serializable]
    public class SpawnGroup
    {
        public EnemyType WhichEnemy;
        [Range(1, 500)]
        public int MaxEnemyCount = 10;

        [Range(0.05f, 10f)]
        public float SpawnDelay = 0.1f;

        [Tooltip("Curva normalizada para spawn do inimigo.")]
        //Pense como uma série de "pesos" ao longo do tempo
        public AnimationCurve SpawnOverTime;
    }
}