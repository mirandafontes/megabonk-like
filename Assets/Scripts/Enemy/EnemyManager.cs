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
                this.transform
            );

            // 2. Pré-preenche a lista usando a referência do Inspector
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

        public void SpawnRandomEnemy(Vector3 spawnPos)
        {
            if (availableBlueprints == null || availableBlueprints.Count == 0)
            {
                Debug.LogError("[EnemyManager] No EnemyBlueprintSO available to spawn.");
                return;
            }

            int randomIndex = Random.Range(0, availableBlueprints.Count);
            EnemyBlueprint blueprint = availableBlueprints[randomIndex];

            SpawnNewEnemy(blueprint, spawnPos);
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
                controller.InitializeData(defaultBlueprint, controller.transform.position, newIndex);

                activeControllers.Add(controller);

                controller.gameObject.SetActive(true);
            }
        }
    }
}