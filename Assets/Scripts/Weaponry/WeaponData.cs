using ScriptableObjects;
using UnityEngine;

namespace Weaponry
{
    /// <summary>
    /// WeaponData representa o estado atual da arma.
    /// É uma classe com dados mutáveis
    /// e um blueprint de referência.
    /// Ao contrário do EnemyData, que tem uma virulência maior,
    /// aqui os dados são mais "centralizados" no blueprint.
    /// </summary>
    [System.Serializable]
    public class WeaponData
    {
        private readonly WeaponBlueprint blueprint;

        public WeaponBlueprint Blueprint => blueprint;
        public int CurrentLevel { get; private set; }
        public float CurrentDamage { get; private set; }
        public float CurrentCooldown { get; private set; }
        public int CurrentAmount { get; private set; }
        public string Name => blueprint.WeaponName;
        public string Description => blueprint.Description;
        public bool IsAutomatic => blueprint.AutoAttack;
        private float nextAttackTime = 0f;

        public WeaponData(WeaponBlueprint blueprint)
        {
            this.blueprint = blueprint;

            // Inicializa com os valores de Level 1
            CurrentLevel = 1;
            CurrentDamage = blueprint.BaseDamage;
            CurrentCooldown = blueprint.BaseCooldown;
            CurrentAmount = blueprint.BaseAmount;
        }

        /// <summary>
        /// Aplica o upgrade e atualiza os valores mutáveis.
        /// </summary>
        public bool TryLevelUp()
        {
            if (CurrentLevel >= blueprint.MaxLevel)
            {
                return false;
            }

            CurrentLevel++;
            UpdateStatsFromCurves();

            return true;
        }

        public bool TryAttack(Vector3 firePosition, Quaternion rotation)
        {
            if (Time.time < nextAttackTime)
            {
                return false;
            }

            if (blueprint.WeaponAttack != null)
            {
                blueprint.WeaponAttack.GetWeaponEffect().Execute(this, firePosition, rotation);
            }

            if (blueprint.WeaponEffect != null)
            {
                blueprint.WeaponEffect.GetWeaponEffect().Execute(this, firePosition, rotation);
            }

            nextAttackTime = Time.time + CurrentCooldown;

            return true;
        }

        public (float, float, float)? GetNextLevelData()
        {
            int nextLevel = Mathf.Clamp(CurrentLevel + 1, 0, blueprint.MaxLevel);
            float newLevelDmg = blueprint.GetValueAtLevel(blueprint.DamageCurve, nextLevel);
            float newLevelCooldown = blueprint.GetValueAtLevel(blueprint.CooldownCurve, nextLevel);
            float newLevelAmount = Mathf.RoundToInt(blueprint.GetValueAtLevel(blueprint.AmountCurve, nextLevel));

            return (newLevelDmg, newLevelCooldown, newLevelAmount);
        }

        private void UpdateStatsFromCurves()
        {
            CurrentDamage = blueprint.GetValueAtLevel(blueprint.DamageCurve, CurrentLevel);
            CurrentCooldown = blueprint.GetValueAtLevel(blueprint.CooldownCurve, CurrentLevel);
            CurrentAmount = Mathf.RoundToInt(blueprint.GetValueAtLevel(blueprint.AmountCurve, CurrentLevel));
        }
    }
}