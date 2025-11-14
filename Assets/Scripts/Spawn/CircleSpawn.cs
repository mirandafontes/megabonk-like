using UnityEngine;

namespace Spawn
{
    public class CircleSpawn : ISpawner
    {
        private readonly float minSpawnRadius = 15f;
        private readonly float maxSpawnRadius = 25f;
        private readonly float exclusionAngle = 45f;

        private const int MaxTries = 2;

        public Vector3 GetSpawnPosition(Transform playerTransform)
        {
            Vector3 playerPosition = playerTransform.position;

            for (int i = 0; i < MaxTries; i++)
            {
                float distance = Random.Range(minSpawnRadius, maxSpawnRadius);
                float angle = Random.Range(0f, 360f);

                float x = Mathf.Cos(angle * Mathf.Deg2Rad) * distance;
                float z = Mathf.Sin(angle * Mathf.Deg2Rad) * distance;

                Vector3 potentialPosition = playerPosition + new Vector3(x, 0f, z);

                if (exclusionAngle > 0)
                {
                    Vector3 spawnDirection = (potentialPosition - playerPosition).normalized;
                    Vector3 playerForward = playerTransform.forward;

                    float currentAngle = Vector3.Angle(playerForward, spawnDirection);

                    if (currentAngle < exclusionAngle / 2f)
                    {
                        continue;
                    }
                }

                return potentialPosition;
            }

            Debug.LogError("[CircleSpawn] Spawn position at fallback case");
            return playerPosition + new Vector3(Random.Range(minSpawnRadius, maxSpawnRadius), 0f, 0f);
        }
    }
}