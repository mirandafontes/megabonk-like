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

        public int CurrentLevel { get; private set; }

        public float CurrentDamage { get; private set; }
        public float CurrentCooldown { get; private set; }
        public int CurrentAmount { get; private set; }
        public string Name => blueprint.WeaponName;
        public string Description => blueprint.Description;

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

        private void UpdateStatsFromCurves()
        {
            CurrentDamage = blueprint.GetValueAtLevel(blueprint.DamageCurve, CurrentLevel);
            CurrentCooldown = blueprint.GetValueAtLevel(blueprint.CooldownCurve, CurrentLevel);
            CurrentAmount = Mathf.RoundToInt(blueprint.GetValueAtLevel(blueprint.AmountCurve, CurrentLevel));
        }
    }
}