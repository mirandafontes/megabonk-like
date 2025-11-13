using System.Collections.Generic;
using GenericPool;
using ScriptableObjects;
using UnityEngine;

namespace Enemy
{
    /// <summary>
    /// Monobehaviour que atua como ponte entre a Unity e os dados POCO.
    /// Prefab do Enemy.
    /// </summary>
    public class EnemyController : MonoBehaviour, IPoolable
    {
        [Header("- Components -")]
        [SerializeField] private Rigidbody rb;

        [Header("- Visual -")]
        [SerializeField] private List<EnemyVisualEntry> enemyVisuals;

        public EnemyData CurrentData { get; private set; }
        public bool IsDataValid { get; private set; }
        //Salvando o index para evitar uso de métodos lineares do LINQ.
        public int Index { get; set; }

        public void InitializeData(EnemyBlueprint newData, Vector3 spawnPos, int index)
        {
            if (CurrentData == null)
            {
                CurrentData = new EnemyData(newData, spawnPos);
            }
            else
            {
                CurrentData.NewEnemyData(newData, spawnPos);
            }

            SetVisual(newData.EnemyType);

            Index = index;
            IsDataValid = true;
        }

        public void OnGetFromPool()
        {
            gameObject.SetActive(true);
        }

        public void OnReturnToPool()
        {
            IsDataValid = false;
            Index = -1;
            gameObject.SetActive(false);
        }

        private void SetVisual(EnemyType type)
        {
            foreach (var entry in enemyVisuals)
            {
                bool isActive = entry.Type == type;
                
                entry.VisualRoot.SetActive(isActive);
                
                if (isActive)
                {
                    Debug.Log($"[EnemyController] Activating {type} visual");
                }
            }
        }

        //Pequena struct para auxiliar na serialização no editor.
        [System.Serializable]
        public struct EnemyVisualEntry
        {
            public EnemyType Type;
            public GameObject VisualRoot;
        }
    }
}