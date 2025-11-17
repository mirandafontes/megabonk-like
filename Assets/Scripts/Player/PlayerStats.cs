using System;
using UnityEngine;
using ScriptableObjects;
using System.Collections.Generic;

namespace Player
{
    /// <summary>
    /// Struct para construção do PlayerStats.
    /// Parameter Object Pattern.
    /// </summary>
    public struct PlayerStatsDependencies
    {
        public MovementSettings MovementSetup;
        public CurveScriptable ExperienceCurve;
        public CurveScriptable HealthCurve;
        public CurveScriptable SpeedCurve;

        public Action<int> OnLevelUpAction;
    }

    /// <summary>
    /// Classe POCO para gerenciar os status do jogador.
    /// Possui comportamento próprio para avançar
    /// os stats base de acordo com a curva
    /// de experiência apresentada.
    /// </summary>
    [Serializable]
    public class PlayerStats
    {
        // -- Curves -- //

        private readonly MovementSettings movementSettings;
        private readonly CurveScriptable experienceBlueprint;
        private readonly CurveScriptable baseHealthUpBlueprint;
        private readonly CurveScriptable baseSpeedUpBlueprint;

        // -- Speed -- //

        private readonly List<float> speedModifiers = new List<float>();
        private float speedModifier = 1f;
        /// <summary>
        /// Velocidade base * modificadores.
        /// </summary>
        public float MaxSpeed
        {
            get
            {
                return Mathf.Ceil(BaseSpeed * speedModifier);
            }
        }

        private float BaseSpeed { get; set; }
        public float AccelerationForce { get; private set; }
        public float InertiaDamping { get; private set; }

        // -- Speed -- //

        // -- Health -- //
        private readonly List<float> healthModifiers = new List<float>();
        private float healthModifier = 1f;
        private float BaseHealth { get; set; }

        /// <summary>
        /// Vida base * modificadores.
        /// </summary>
        public float MaxHealth
        {
            get
            {
                return Mathf.Ceil(BaseHealth * healthModifier);
            }
        }

        public float CurrentHealth { get; set; }

        // -- Health -- //


        // -- Experience -- //

        private Action<int> onLevelUpReached;

        private int currentLevel = 1;
        private int totalExperience = 0;

        public int CurrentLevel => currentLevel;
        public int TotalExperience => totalExperience;
        public bool IsMaxLevel => currentLevel >= experienceBlueprint.GetLastPoint().Item1;

        // -- Experience -- //

        public PlayerStats(PlayerStatsDependencies dependencies)
        {
            movementSettings = dependencies.MovementSetup;
            experienceBlueprint = dependencies.ExperienceCurve;
            baseHealthUpBlueprint = dependencies.HealthCurve;
            baseSpeedUpBlueprint = dependencies.SpeedCurve;
            onLevelUpReached = dependencies.OnLevelUpAction;

            BaseSpeed = movementSettings.BaseSpeed;
            AccelerationForce = movementSettings.AccelerationForce;
            InertiaDamping = movementSettings.InertiaDamping;

            float initialMaxHealth = baseHealthUpBlueprint.GetValueAtLevel(currentLevel);

            BaseHealth = initialMaxHealth;
            CurrentHealth = initialMaxHealth;
        }

        private void RefreshStatsFromCurves()
        {
            float newBaseSpeed = baseSpeedUpBlueprint.GetValueAtLevel(currentLevel);

            BaseSpeed = newBaseSpeed;
            RecalculateMovementSpeed();

            float newBaseHealth = baseHealthUpBlueprint.GetValueAtLevel(currentLevel);

            float oldBaseHealth = BaseHealth;
            BaseHealth = newBaseHealth;
            RecalculateHealth();

            float healthIncrease = BaseHealth - oldBaseHealth;
            CurrentHealth += healthIncrease;
        }

        #region Speed
        public void AddSpeedModifier(float percentageIncrease)
        {
            speedModifiers.Add(percentageIncrease);
            RecalculateMovementSpeed();
        }

        public void RemoveSpeedModifier(float percentageIncrease)
        {
            speedModifiers.Remove(percentageIncrease);
            RecalculateMovementSpeed();
        }

        public void ResetSpeedModifier()
        {
            speedModifiers.Clear();
            RecalculateMovementSpeed();
        }

        private void RecalculateMovementSpeed()
        {
            float totalModifier = 1.0f;
            foreach (var mod in speedModifiers)
            {
                totalModifier += mod;
            }
            speedModifier = totalModifier;
        }
        #endregion

        #region Health
        public void AddHealthModifier(float percentageIncrease)
        {
            healthModifiers.Add(percentageIncrease);
            RecalculateHealth();
        }

        public void RemoveHealthModifier(float percentageIncrease)
        {
            healthModifiers.Remove(percentageIncrease);
            RecalculateHealth();
        }

        public void ResetHealthModifier()
        {
            healthModifiers.Clear();
            RecalculateHealth();
        }

        private void RecalculateHealth()
        {
            float totalModifier = 1.0f;
            foreach (var mod in healthModifiers)
            {
                totalModifier += mod;
            }
            healthModifier = totalModifier;
        }
        #endregion

        #region Experience

        public void AddExperience(int amount)
        {
            if (amount <= 0 || IsMaxLevel)
            {
                Debug.Log("[PlayerStats] Player reached max level");
                return;
            }

            totalExperience += amount;

            //Para caso haja muita experiência acumulada
            if (CheckForLevelUp())
            {
                RefreshStatsFromCurves();
                onLevelUpReached?.Invoke(currentLevel);
            }
        }

        /// <summary>
        /// Calcula a experiência total acumulada necessária para atingir um nível.
        /// </summary>
        public int GetRequiredTotalExperienceFor(int targetLevel)
        {
            if (targetLevel <= 1)
            {
                return 0;
            }

            float levelIndex = Mathf.Clamp(targetLevel, 1, experienceBlueprint.GetLastPoint().Item1);

            return Mathf.RoundToInt(experienceBlueprint.GetValueAtLevel(levelIndex));
        }

        /// <summary>
        /// Obtém a razão, do (total de exp acumulada - exp mínima do nível atual) / (exp para próximo nível)
        /// </summary>
        /// <returns></returns>
        public float GetCurrentLevelProgress()
        {
            if (IsMaxLevel)
            {
                return 1f;
            }

            int requiredForCurrentLevelBase = GetRequiredTotalExperienceFor(CurrentLevel);
            int requiredForNextLevelTotal = GetRequiredTotalExperienceFor(CurrentLevel + 1);
            int experienceGainedInCurrentLevel = TotalExperience - requiredForCurrentLevelBase;
            int requiredExpDelta = requiredForNextLevelTotal - requiredForCurrentLevelBase;

            if (requiredExpDelta <= 0)
            {
                return 1f;
            }

            float progress = (float)experienceGainedInCurrentLevel / requiredExpDelta;
            return Mathf.Clamp01(progress);
        }

        /// <summary>
        /// Verifica se a experiência total acumulada é suficiente para o próximo nível.
        /// </summary>
        private bool CheckForLevelUp()
        {
            if (IsMaxLevel)
            {
                return false;
            }

            int requiredXPForNextLevel = GetRequiredTotalExperienceFor(currentLevel + 1);

            if (totalExperience >= requiredXPForNextLevel)
            {
                currentLevel++;
                return true;
            }

            return false;
        }
        #endregion
    }
}