using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enemy;
using ScriptableObjects;
using Spawn;
using UnityEngine;

namespace Wave
{
    public class WaveManager : MonoBehaviour
    {
        [Header("- Dependencies -")]
        [SerializeField] private EnemyManager enemyManager;
        [SerializeField] private WavesScriptable wavesData;
        [SerializeField] private Transform playerTransform;

        private ISpawner spawnPositionGenerator;
        private int currentWaveIndex = 0;
        private float waveStartTime;

        private readonly WaitForSeconds waitOneTenth = new WaitForSeconds(0.1f);
        private List<SpawnGroupTracker> trackers = new List<SpawnGroupTracker>();
        private int activeTrackerCount;

        #region Unity
        private void Awake()
        {
            //Novamente, poderíamos aplicar uma strategy pattern com uma factory
            //Ou criar um scriptable object.
            //Ou, ainda, estipular o algortimo de spawn para cada tipo de monstro.
            spawnPositionGenerator = new CircleSpawn();
        }
        #endregion

        [ContextMenu("Start Game")]
        public void StartGame()
        {
            if (wavesData != null && enemyManager != null && spawnPositionGenerator != null)
            {
                StartCoroutine(WaveCycleCoroutine());
            }
            else
            {
                Debug.LogError("[WaveManager] WaveManager is not configured correctly. Check all dependencies.");
            }
        }

        private IEnumerator WaveCycleCoroutine()
        {
            while (currentWaveIndex < wavesData.Waves.Count)
            {
                WaveSetup currentWave = wavesData.Waves[currentWaveIndex];

                Debug.Log($"[WaveManager] Waiting {currentWave.InitalTimeBeforeSpawn}s before initial spawn.");

                // Como é variável, não dá para cachear
                yield return new WaitForSeconds(currentWave.InitalTimeBeforeSpawn);

                Debug.Log($"[WaveManager] Starting Wave Spawn {currentWaveIndex + 1}.");
                waveStartTime = Time.time;

                yield return StartCoroutine(ManageWaveDuration(currentWave));

                Debug.Log("[WaveManager] Spawn time expired. Awaiting enemy elimination.");

                yield return new WaitUntil(AllEnemiesEliminated);

                // Igualmente
                yield return new WaitForSeconds(currentWave.CooldownTime);

                currentWaveIndex++;
            }

            Debug.Log("[WaveManager] All Waves Completed. Game Over!");
        }

        private IEnumerator ManageWaveDuration(WaveSetup setup)
        {
            float waveEndTime = waveStartTime + setup.TotalTime;
            activeTrackerCount = setup.Groups.Count;

            //Cacheando os trackers atuais para evitar uso de linq
            trackers.Clear();
            foreach (var group in setup.Groups)
            {
                trackers.Add(new SpawnGroupTracker(group));
            }

            while (Time.time < waveEndTime || activeTrackerCount > 0)
            {
                ProcessTrackers(trackers, setup);

                yield return waitOneTenth;
            }

            trackers.Clear();
        }

        private void AttemptSpawn(SpawnGroupTracker tracker, float currentTimeInWave, int totalWaveTime)
        {
            SpawnGroup group = tracker.Group;

            if (tracker.SpawnedCount >= group.MaxEnemyCount)
            {
                if (tracker.IsActive)
                {
                    tracker.IsActive = false;
                    activeTrackerCount--;
                }
                return;
            }

            if (Time.time < tracker.NextSpawnTime)
            {
                return;
            }

            float normalizedTime = currentTimeInWave / totalWaveTime;
            float spawnWeight = group.SpawnOverTime.Evaluate(normalizedTime);

            if (Random.value <= spawnWeight)
            {
                Vector3 spawnPos = spawnPositionGenerator.GetSpawnPosition(playerTransform);

                enemyManager.SpawnEnemy(
                    group.WhichEnemy,
                    spawnPos
                );

                tracker.IncrementSpawn();
                tracker.NextSpawnTime = Time.time + group.SpawnDelay;
            }
        }

        private void ProcessTrackers(List<SpawnGroupTracker> trackers, WaveSetup setup)
        {
            float currentTimeInWave = Time.time - waveStartTime;

            foreach (var tracker in trackers)
            {
                if (!tracker.IsActive)
                {
                    continue;
                }

                if (enemyManager.GetActiveEnemyCount() < setup.MaxEnemies)
                {
                    AttemptSpawn(tracker, currentTimeInWave, setup.TotalTime);
                }
            }
        }

        private bool AllEnemiesEliminated()
        {
            return enemyManager.GetActiveEnemyCount() == 0; 
        }
    }
}