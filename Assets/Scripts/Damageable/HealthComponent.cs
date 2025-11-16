using System;
using UnityEngine;

namespace Damageable
{
    /// <summary>
    /// Componente Monobehaviour reutilizável para coisas com vida.
    /// </summary>
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

        private Action<float> onHit;
        private Action onDeath;

        //Initialize está separado de SetHealth para garantir unicidade nas actions registradas.
        public void Initialize(Action<float> onHit, Action onDeath)
        {
            this.onHit = onHit;
            this.onDeath = onDeath;
        }

        public void SetHealth(float maxHealth, float currentHealth)
        {
            this.maxHealth = maxHealth;
            this.currentHealth = currentHealth;
        }

        public void TakeDamage(float amount)
        {
            if (currentHealth <= 0)
            {
                return;
            }

            //Como o dano é tratado, é passado para as partes
            onHit?.Invoke(amount);

            if (currentHealth <= 0)
            {
                onDeath?.Invoke();
                return;
            }
        }
    }
}