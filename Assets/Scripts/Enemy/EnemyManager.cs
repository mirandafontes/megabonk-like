using System.Collections.Generic;
using Event;
using GenericPool;
using Pursuit;
using ScriptableObjects;
using UnityEngine;

namespace Enemy
{
    public class EnemyManager : MonoBehaviour
    {
        private readonly List<EnemyController> activeControllers = new List<EnemyController>();
        private readonly List<EnemyController> toRemove = new List<EnemyController>(50);
        private MonoBehaviourPool<EnemyController> enemyPool;

        [Header("- Pool -")]
        [SerializeField] private Transform enemiesParent;
        [SerializeField] private EnemyController enemyPrefab;
        [SerializeField] private int initialPoolSize = 250;
        [SerializeField] private List<EnemyBlueprint> blueprints;
        private readonly Dictionary<EnemyType, EnemyBlueprint> mappedBlueprints = new Dictionary<EnemyType, EnemyBlueprint>();

        [Header("- Scene pool -")]
        [SerializeField] private List<EnemyController> prePlacedInitialEnemies;

        [Header("- Dummy -")]
        [SerializeField] private Transform playerTransform;

        [Header("- Movement algorithm -")]
        [SerializeField] private PursuitStrategy fallbackPursuit;
        [SerializeField] private int enemiesPerFrame = 5;
        private int nextEnemyIndex = 0;

        #region Unity
        private void Awake()
        {
            InitializePool();
            MapEnemyBlueprints();
            PrecacheEnemies();
            FillPoolToInitialSize();
        }

        private void FixedUpdate()
        {
            if (playerTransform == null || activeControllers.Count <= 0)
            {
                return;
            }

            UpdateEnemies(Time.fixedDeltaTime);
        }
        #endregion

        public void SpawnEnemy(EnemyType enemyType, Vector3 spawnPos)
        {
            if (!mappedBlueprints.ContainsKey(enemyType))
            {
                Debug.LogError($"[EnemyManager] Trying spawn {enemyType}, but it's not mapped in blueprint dictionary.");
                return;
            }

            EnemyBlueprint blueprint = mappedBlueprints[enemyType];
            EnemyController controller = enemyPool.Get();

            int newIndex = activeControllers.Count;
            controller.InitializeData(blueprint, spawnPos, newIndex);

            activeControllers.Add(controller);
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

        public int GetActiveEnemyCount()
        {
            return activeControllers?.Count ?? 0;
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

        private void UpdateEnemies(float deltaTime)
        {
            Vector3 playerPos = playerTransform.position;
            int activeCount = activeControllers.Count;
            int enemiesToProcess = Mathf.Min(enemiesPerFrame, activeCount);

            if (nextEnemyIndex >= activeControllers.Count)
            {
                nextEnemyIndex = 0;
            }

            toRemove.Clear();

            //Atualizamos apenas parte dos inimigos em determinado frame
            //Para dividir a atualização de movimentação ao longo de diversos frames
            for (int i = 0; i < enemiesToProcess; i++)
            {
                int currentIndex = nextEnemyIndex;

                if (nextEnemyIndex >= activeCount)
                {
                    nextEnemyIndex = 0;
                    break;
                }

                //Short-circuit
                EnemyController controller = activeControllers[currentIndex];
                if (!controller.IsDataValid || controller.IsKnockingBack)
                {
                    nextEnemyIndex++;
                    continue;
                }

                if (controller.IsDying)
                {
                    toRemove.Add(controller);
                    nextEnemyIndex++;
                    continue;
                }

                IPursuit enemyPursuit = controller.GetPursuit();
                IPursuit pursuit = enemyPursuit ?? fallbackPursuit.GetBehavior();

                MovementResult result = pursuit.CalculateMovement(
                    controller.Position,
                    playerPos
                );

                controller.ApplyMovement(result.FinalDirection, result.IsAvoidingObstacle, deltaTime);
                nextEnemyIndex++;
            }

            if (toRemove.Count > 0)
            {
                int totalExp = 0;
                int totalCount = toRemove.Count;

                for (int i = 0; i < toRemove.Count; i++)
                {
                    totalExp += toRemove[i].Experience;
                    KillAndReturn(toRemove[i]);
                }
                
                EventBus.Publish(new OnEnemyDeathEvent
                {
                    TotalExperience = totalExp,
                    TotalEnemiesKilled = totalCount,
                });
            }
        }

        private void InitializePool()
        {
            enemyPool = new MonoBehaviourPool<EnemyController>(
                () => Instantiate(enemyPrefab, enemiesParent),
                initialPoolSize,
                true,
                enemiesParent
            );
        }

        private void MapEnemyBlueprints()
        {
            //Mapeando blueprints para acesso mais rápido
            for (int i = 0; i < blueprints.Count; i++)
            {
                mappedBlueprints.TryAdd(blueprints[i].EnemyType, blueprints[i]);
            }
        }

        private void PrecacheEnemies()
        {
            if (prePlacedInitialEnemies.Count > 0)
            {
                InitializePrePlacedEnemies();
            }
        }

        private void InitializePrePlacedEnemies()
        {
            List<EnemyController> toAddToPool = new List<EnemyController>();
            EnemyBlueprint skeletonBlueprint = mappedBlueprints[EnemyType.Skeleton];

            foreach (var controller in prePlacedInitialEnemies)
            {
                if (controller == null)
                {
                    continue;
                }

                controller.InitializeData(skeletonBlueprint, Vector3.zero, -1);

                controller.gameObject.SetActive(false);
                controller.transform.SetParent(enemiesParent);

                toAddToPool.Add(controller);
            }

            enemyPool.AddExistingItemsToPool(toAddToPool);
            prePlacedInitialEnemies.Clear();
        }

        private void FillPoolToInitialSize()
        {
            int currentSize = enemyPool.CurrentAvailableCount + (activeControllers?.Count ?? 0);
            int objectsToInstantiate = initialPoolSize - currentSize;

            if (objectsToInstantiate <= 0)
            {
                return;
            }

            for (int i = 0; i < objectsToInstantiate; i++)
            {
                var newObj = Instantiate(enemyPrefab, enemiesParent);
                enemyPool.ReturnToPool(newObj);
            }
        }
    }
}
