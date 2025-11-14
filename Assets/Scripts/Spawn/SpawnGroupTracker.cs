using UnityEngine;

namespace Wave
{
    /// <summary>
    /// Classe POCO para manter tracking das informações de spawn.
    /// </summary>
    public class SpawnGroupTracker
    {
        public SpawnGroup Group { get; }
        public int SpawnedCount { get; private set; }
        public float NextSpawnTime { get; set; }
        public bool IsActive { get; set; }

        public SpawnGroupTracker(SpawnGroup group)
        {
            Group = group;
            SpawnedCount = 0;
            NextSpawnTime = Time.time;
            IsActive = true;
        }

        public void IncrementSpawn()
        {
            SpawnedCount++;
        }
    }
}