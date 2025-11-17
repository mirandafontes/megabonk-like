using UnityEngine;
using ScriptableObjects;
using Damageable;
using Event;

namespace Player
{
    /// <summary>
    /// Monobehaviour que atua como ponte entre a Unity e os dados POCO.
    /// Prefab do Player.
    /// Classe orquestradora.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        [Header("- Setup -")]
        [SerializeField] private MovementSettings movementSetup;
        [SerializeField] private CurveScriptable experienceCurve;
        [SerializeField] private CurveScriptable healthCurve;
        [SerializeField] private CurveScriptable speedCurve;

        [Header("- Dependencies -")]
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private HealthComponent healthComponent;

        [Header("- Data -")]
        [SerializeField]
        [Tooltip("Debug")]
        private PlayerStats playerStats;

        #region Unity
        private void Awake()
        {
            playerStats = new PlayerStats(new PlayerStatsDependencies
            {
                MovementSetup = movementSetup,
                ExperienceCurve = experienceCurve,
                HealthCurve = healthCurve,
                SpeedCurve = speedCurve,
                OnLevelUpAction = OnLevelUp,
            });

            //Injeção da dependencia a partir da classe orquestradora.
            playerMovement.Initialize(playerStats);
            healthComponent.Initialize(OnHit, OnDeath);
            healthComponent.SetHealth(playerStats.CurrentHealth, playerStats.MaxHealth);

            SubscribeOnEnemyDeath();
        }

        private void OnDestroy()
        {
            UnsubscribeOnEnemyDeath();
        }
        #endregion

        public PlayerStats GetStats()
        {
            return playerStats;
        }

        private void OnLevelUp(int nextLevel)
        {
            Debug.Log($"[PlayerController] Level Up!. New Level: {nextLevel} ");
            EventBus.Publish(new OnPlayerLevelUp());
        }

        private void OnHit(float damage)
        {
            playerStats.CurrentHealth -= damage;
            healthComponent.CurrentHealth = playerStats.CurrentHealth;

            EventBus.Publish(new OnPlayerHit());
        }

        private void OnDeath()
        {
            Debug.Log($"[PlayerController] Player is no more.");
            gameObject.SetActive(false);

            EventBus.Publish(new OnPlayerDeath());
        }

        private void SubscribeOnEnemyDeath()
        {
            EventBus.Subscribe<OnEnemyDeathEvent>(OnEnemyDeath);
        }
        private void UnsubscribeOnEnemyDeath()
        {
            EventBus.Unsubscribe<OnEnemyDeathEvent>(OnEnemyDeath);
        }

        private void OnEnemyDeath(OnEnemyDeathEvent data)
        {
            Debug.Log($"[PlayerController] Receive {data.TotalExperience} experience from {data.TotalEnemiesKilled} enemies");

            playerStats.AddExperience(data.TotalExperience);
            EventBus.Publish(new OnPlayerAcquireExp());
        }
    }
}