using System;
using UnityEngine;

namespace Damageable
{
    public class HealthComponent : MonoBehaviour, IDamageable
    {
        [Header("- Data -")]
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth;
        public float CurrentHealth => currentHealth;

        private Action onHit;
        private Action onDeath;

        public void Initialize(float maxHealth, float currentHealth)
        {
            this.maxHealth = maxHealth;
            this.currentHealth = currentHealth;
        }

        public void BindActions(Action onHit, Action onDeath)
        {
            this.onHit = onHit;
            this.onDeath = onDeath;
        }

        public float GetNormalizedHealth()
        {
            return Mathf.Clamp(currentHealth / maxHealth, 0f, 1f);
        }

        public void TakeDamage(float amount, GameObject instigator)
        {
            if (currentHealth <= 0)
            {
                return;
            }

            currentHealth -= amount;

            onHit?.Invoke();

            if (currentHealth <= 0)
            {
                onDeath?.Invoke();
                return;
            }
        }
    }
}