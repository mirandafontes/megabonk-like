using System.Collections.Generic;
using GenericPool;
using ScriptableObjects;
using Unity.VisualScripting;
using UnityEngine;

namespace Enemy
{
    public class EnemyManager : MonoBehaviour
    {
        public static EnemyManager Instance { get; private set; }

        private List<EnemyController> activeControllers = new List<EnemyController>();

        [Header("- Pool -")]
        [SerializeField] private Transform enemiesParent;
        [SerializeField] private EnemyController enemyPrefab;
        [SerializeField] private int initialPoolSize = 250;

        [Header("- Blueprints - ")]
        [SerializeField] private List<EnemyBlueprint> availableBlueprints;
        private MonoBehaviourPool<EnemyController> enemyPool;

        [Header("- Scene pool -")]
        [SerializeField] private List<EnemyController> _prePlacedInitialEnemies;
        [SerializeField] private EnemyBlueprint defaultBlueprint;

        [Header("- Dummy -")]
        [SerializeField] private Transform playerTransform;

        [Header("- Spawn config -")]
        [SerializeField] private float minSpawnRadius = 15f;
        [SerializeField] private float maxSpawnRadius = 25f;
        [SerializeField] private float exclusionAngle = 45f;

        #region Unity
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            enemyPool = new MonoBehaviourPool<EnemyController>(
                () => Instantiate(enemyPrefab, this.transform).GetComponent<EnemyController>(),
                initialPoolSize,
                true,
                enemiesParent
            );

            if (_prePlacedInitialEnemies.Count > 0 && defaultBlueprint != null)
            {
                InitializePrePlacedEnemies();
            }

            //_playerTransform = FindObjectOfType<PlayerController>().transform;
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                return;
            }

            Vector3 playerPos = playerTransform.position;

            for (int i = activeControllers.Count - 1; i >= 0; i--)
            {
                EnemyController controller = activeControllers[i];
                EnemyData enemyData = controller.CurrentData;

                Vector3 direction = (playerPos - enemyData.Position).normalized;
                enemyData.Position += enemyData.CurrentSpeed * Time.deltaTime * direction;

                controller.transform.position = enemyData.Position;
            }
        }
        #endregion

        public void SpawnNewEnemy(EnemyBlueprint blueprint, Vector3 spawnPos)
        {
            EnemyController controller = enemyPool.Get();

            int newIndex = activeControllers.Count;
            controller.InitializeData(blueprint, spawnPos, newIndex);

            activeControllers.Add(controller);
        }

        [ContextMenu("Spawn random enemy")]
        public void SpawnRandomEnemy()
        {
            if (availableBlueprints == null || availableBlueprints.Count == 0)
            {
                Debug.LogError("[EnemyManager] No EnemyBlueprintSO available to spawn.");
                return;
            }

            int randomIndex = Random.Range(0, availableBlueprints.Count);
            EnemyBlueprint blueprint = availableBlueprints[randomIndex];

            Vector3 newSpawn = CalculateSpawnPosition(playerTransform.position);

            SpawnNewEnemy(blueprint, newSpawn);
        }

        public void HandleDamage(EnemyController controller, float damage)
        {
            if (controller == null || controller.IsDataValid == false)
            {
                return;
            }

            EnemyData enemyData = controller.CurrentData;
            enemyData.CurrentHealth -= damage;

            if (enemyData.CurrentHealth <= 0)
            {
                KillAndReturn(controller);
            }
        }

        [ContextMenu("Kill random enemy")]
        public void KillRandomEnemy()
        {
            if (activeControllers.Count == 0)
            {
                Debug.Log("[EnemyManger] No active enemies to kill.");
                return;
            }

            int randomIndex = Random.Range(0, activeControllers.Count);
            EnemyController controllerToKill = activeControllers[randomIndex];

            KillAndReturn(controllerToKill);
        }

        private void KillAndReturn(EnemyController controller)
        {
            int index = controller.Index;

            if (index <= -1)
            {
                Debug.LogError("[EnemyManager] Invalid index at Kill and Return, using fallback.");
                index = activeControllers.IndexOf(controller);
            }

            if (index <= -1)
            {
                Debug.LogError("[EnemyManager] Invalid index at fallback.");
                return;
            }

            int lastIndex = activeControllers.Count - 1;

            enemyPool.ReturnToPool(controller);

            if (index != lastIndex)
            {
                EnemyController swappedController = activeControllers[lastIndex];
                activeControllers[index] = swappedController;

                swappedController.Index = index;
            }

            activeControllers.RemoveAt(lastIndex);
        }

        private void InitializePrePlacedEnemies()
        {
            foreach (var controller in _prePlacedInitialEnemies)
            {
                if (controller == null)
                {
                    continue;
                }

                int newIndex = activeControllers.Count;
                Vector3 spawnPosition = CalculateSpawnPosition(playerTransform.position);
                controller.InitializeData(defaultBlueprint, spawnPosition, newIndex);

                activeControllers.Add(controller);

                controller.gameObject.SetActive(true);
            }
        }

        private Vector3 CalculateSpawnPosition(Vector3 playerPosition)
        {
            for (int i = 0; i < 5; i++)
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

            Debug.LogError("[EnemyManager] Spawn position at fallback case");
            return playerPosition + new Vector3(Random.Range(minSpawnRadius, maxSpawnRadius), 0f, 0f);
        }
    }
}