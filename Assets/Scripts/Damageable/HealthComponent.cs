using System;
using UnityEngine;

namespace Damageable
{
    public class HealthComponent : MonoBehaviour, IDamageable
    {
        [Header("- Data -")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;
        public float CurrentHealth
        {
            get
            {
                return currentHealth;

            }

            set
            {
                currentHealth = value;
            }

        }
        public float NormalizedHealth
        {
            get
            {
                return Mathf.Clamp(currentHealth / maxHealth, 0f, 1f);
            }
        }

        private Action<float, GameObject> onHit;
        private Action onDeath;

        public void Initialize(float maxHealth, float currentHealth)
        {
            this.maxHealth = maxHealth;
            this.currentHealth = currentHealth;
        }

        public void BindActions(Action<float, GameObject> onHit, Action onDeath)
        {
            this.onHit = onHit;
            this.onDeath = onDeath;
        }

        public void TakeDamage(float amount, GameObject instigator)
        {
            if (currentHealth <= 0)
            {
                return;
            }

            //Como o dano é tratado, é passado para as partes
            onHit?.Invoke(amount, instigator);

            if (currentHealth <= 0)
            {
                onDeath?.Invoke();
                return;
            }
        }
    }
}