using System.Collections;
using System.Collections.Generic;
using GenericPool;
using ScriptableObjects;
using UnityEngine;
using Damageable;

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
        [SerializeField] private HealthComponent healthComponent;

        [Header("- Visual -")]
        [SerializeField] private List<EnemyVisualEntry> enemyVisuals;

        public EnemyData CurrentData;
        public bool IsDataValid { get; private set; }
        //Salvando o index para evitar uso de métodos lineares do LINQ.
        public int Index { get; set; }
        public bool IsKnockingBack
        {
            get
            {
                if (CurrentData == null)
                {
                    return false;
                }

                return CurrentData.IsKnockingBack;

            }
            private set
            {
                if (CurrentData == null)
                {
                    return;
                }

                CurrentData.IsKnockingBack = value;
            }
        }

        private Coroutine knockbackCoroutine;

        #region Unity
        private void Awake()
        {
            healthComponent.BindActions(OnHit, OnDeath);
        }
        #endregion

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

            transform.position = CurrentData.Position;
            SetVisual(newData.EnemyType);
            healthComponent.Initialize(CurrentData.CurrentHealth, CurrentData.CurrentHealth);

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

        public void ApplyMovement(Vector3 targetPosition, bool isAvoiding, float deltaTime)
        {
            if (CurrentData.IsDying || CurrentData.IsKnockingBack)
            {
                return;
            }

            CurrentData.IsAvoiding = isAvoiding;

            Vector3 normalizedDirection = targetPosition.normalized;
            CurrentData.Position += CurrentData.CurrentSpeed * deltaTime * normalizedDirection;

            transform.position = CurrentData.Position;

            if (normalizedDirection.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(normalizedDirection),
                    deltaTime * 10
                );
            }
        }

        private void SetVisual(EnemyType type)
        {
            foreach (var entry in enemyVisuals)
            {
                bool isActive = entry.Type == type;

                entry.VisualRoot.SetActive(isActive);
            }
        }

        private void OnHit(float amount, GameObject instigator)
        {
            //Aqui podemos aplicar os fx relacionados ao hit
            CurrentData.CurrentHealth -= amount;
            healthComponent.CurrentHealth = CurrentData.CurrentHealth;

            Knockback(instigator);
        }

        private void OnDeath()
        {
            CurrentData.IsDying = true;
        }

        private void Knockback(GameObject instigator)
        {
            if (CurrentData.IsDying || CurrentData == null)
            {
                return;
            }

            if (IsKnockingBack)
            {
                if (knockbackCoroutine != null)
                {
                    StopCoroutine(knockbackCoroutine);
                }
            }

            float defaultKnockbackForce = 5f;

            Vector3 knockbackDirection = (CurrentData.Position - instigator.transform.position).normalized;
            knockbackCoroutine = StartCoroutine(PerformKnockback(knockbackDirection, defaultKnockbackForce));
        }

        private IEnumerator PerformKnockback(Vector3 direction, float force)
        {
            IsKnockingBack = true;
            float timer = 0f;
            Vector3 startPosition = transform.position;
            Vector3 targetPosition = startPosition + direction * force;
            float knockbackDuration = 0.15f;

            // O movimento será rápido e baseado em Tempo
            while (timer < knockbackDuration)
            {
                timer += Time.deltaTime;
                float t = timer / knockbackDuration;

                //EaseOutCubic
                float t_eased = 1f - Mathf.Pow(1f - t, 3);
                transform.position = Vector3.Lerp(startPosition, targetPosition, t_eased);
                CurrentData.Position = transform.position;

                yield return null;
            }

            IsKnockingBack = false;
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